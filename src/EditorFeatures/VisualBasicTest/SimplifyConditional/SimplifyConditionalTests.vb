﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.SimplifyConditional

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.SimplifyConditional
    Partial Public Class SimplifyConditionalTests
        Inherits AbstractVisualBasicDiagnosticProviderBasedUserDiagnosticTest

        Friend Overrides Function CreateDiagnosticProviderAndFixer(workspace As Workspace) As (DiagnosticAnalyzer, CodeFixProvider)
            Return (New VisualBasicSimplifyConditionalDiagnosticAnalyzer(), New VisualBasicSimplifyConditionalCodeFixProvider())
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestSimpleCase() As Task
            Await TestInRegularAndScript1Async(
"
imports System

class C
    function M() as boolean
        return [|if(X() AndAlso Y(), true, false)|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class",
"
imports System

class C
    function M() as boolean
        return X() AndAlso Y()
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestSimpleNegatedCase() As Task
            Await TestInRegularAndScript1Async(
"
imports System

class C
    function M() as boolean
        return [|if(X() AndAlso Y(), false, true)|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class",
"
imports System

class C
    function M() as boolean
        return Not X() OrElse Not Y()
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestMustBeBool1() As Task
            Await TestMissingInRegularAndScriptAsync(
"
imports System

class C
    function M() as string
        return [|if(X() AndAlso Y(), """", null)|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestMustBeBool2() As Task
            Await TestMissingInRegularAndScriptAsync(
"
imports System

class C
    function M() as string
        return [|if(X() AndAlso Y(), null, """")|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestNotWithTrueTrue() As Task
            Await TestMissingInRegularAndScriptAsync(
"
imports System

class C
    function M() as boolean
        return [|if(X() AndAlso Y(), true, true)|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsSimplifyConditional)>
        Public Async Function TestNotWithFalseFalse() As Task
            Await TestMissingInRegularAndScriptAsync(
"
imports System

class C
    function M() as boolean
        return [|if(X() AndAlso Y(), false, false)|]
    end function

    private function X() as boolean
    private function Y() as boolean
end class")
        End Function
    End Class
End Namespace
