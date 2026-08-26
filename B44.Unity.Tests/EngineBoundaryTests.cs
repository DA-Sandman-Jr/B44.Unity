using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using B44.Common.Diagnostics;
using B44.Unity.Diagnostics;
using Xunit;

namespace B44.Unity.Tests;

/// <summary>
/// The dependency direction, asserted against the built assemblies rather than
/// against the project files that were supposed to produce them.
/// </summary>
/// <remarks>
/// These run on a machine with no Unity installed, and that is the point: every
/// claim here is about what engine-free B44 does and does not carry, which is
/// exactly the property that has to survive without an editor present to check
/// it. Nothing here says the Unity side works; see the proving project.
/// </remarks>
public sealed class EngineBoundaryTests
{
    private static readonly Assembly EngineIndependent = typeof(StructuredGameLogger).Assembly;
    private static readonly Assembly UnityIntegration = typeof(UnityLogRouting).Assembly;

    private static bool IsUnityAssembly(string name) =>
        name.StartsWith("UnityEngine", StringComparison.Ordinal) ||
        name.StartsWith("UnityEditor", StringComparison.Ordinal) ||
        name.Equals("Unity", StringComparison.Ordinal);

    [Fact]
    public void EngineIndependentB44_ResolvesNoUnityAssembly()
    {
        string[] unityReferences = EngineIndependent
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(IsUnityAssembly)
            .ToArray();

        Assert.True(
            unityReferences.Length == 0,
            $"{EngineIndependent.GetName().Name} resolves Unity assemblies ({string.Join(", ", unityReferences)}). " +
            "Engine-free B44 must build and test with no engine present; Unity-facing code belongs in B44.Unity.");
    }

    [Fact]
    public void EngineIndependentB44_DoesNotDependOnTheUnityIntegration()
    {
        string integrationName = UnityIntegration.GetName().Name!;

        Assert.DoesNotContain(
            EngineIndependent.GetReferencedAssemblies(),
            reference => string.Equals(reference.Name, integrationName, StringComparison.Ordinal));
    }

    [Fact]
    public void EngineIndependentB44_ExposesNoUnityTypeInItsPublicApi()
    {
        string[] leaks = PublicSignatureTypes(EngineIndependent)
            .Where(type => IsUnityAssembly(type.Assembly.GetName().Name ?? string.Empty) ||
                           (type.Namespace ?? string.Empty).StartsWith("Unity", StringComparison.Ordinal))
            .Select(type => type.FullName ?? type.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            leaks.Length == 0,
            $"Unity types appear in {EngineIndependent.GetName().Name}'s public API: {string.Join(", ", leaks)}.");
    }

    [Fact]
    public void UnityIntegration_DependsOnTheEngineIndependentPackage()
    {
        // The direction stated the other way round. A boundary that referenced
        // nothing below it would satisfy every "no Unity in Core" assertion above
        // while integrating nothing at all.
        string engineIndependentName = EngineIndependent.GetName().Name!;

        Assert.Contains(
            UnityIntegration.GetReferencedAssemblies(),
            reference => string.Equals(reference.Name, engineIndependentName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Every type reachable from a public or protected signature: base types,
    /// interfaces, fields, properties, events, and method parameters and returns.
    /// </summary>
    private static IEnumerable<Type> PublicSignatureTypes(Assembly assembly)
    {
        const BindingFlags Declared =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (Type type in assembly.GetExportedTypes())
        {
            if (type.BaseType is not null)
            {
                yield return type.BaseType;
            }

            foreach (Type contract in type.GetInterfaces())
            {
                yield return contract;
            }

            foreach (MemberInfo member in type.GetMembers(Declared).Where(IsVisibleOutsideAssembly))
            {
                foreach (Type used in SignatureTypes(member))
                {
                    yield return used;
                }
            }
        }
    }

    private static bool IsVisibleOutsideAssembly(MemberInfo member) => member switch
    {
        FieldInfo field => field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly,
        MethodBase method => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly,
        // Properties and events are visible through their accessors, which are
        // returned separately by GetMembers and checked as MethodBase above.
        _ => false,
    };

    private static IEnumerable<Type> SignatureTypes(MemberInfo member)
    {
        switch (member)
        {
            case FieldInfo field:
                yield return field.FieldType;
                break;
            case MethodInfo method:
                yield return method.ReturnType;
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    yield return parameter.ParameterType;
                }

                break;
            case ConstructorInfo constructor:
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    yield return parameter.ParameterType;
                }

                break;
        }
    }
}
