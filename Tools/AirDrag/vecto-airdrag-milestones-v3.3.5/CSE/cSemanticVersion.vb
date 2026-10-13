''-----------------------------------------------------------------------------
'' <copyright file="SemanticVersion.cs" company="ImaginaryRealities">
'' Copyright 2013 ImaginaryRealities, LLC
'' </copyright>
'' <summary>
'' This file implements the SemanticVersion class. The SemanticVersion class
'' represents a semantic version number for a program.
'' </summary>
'' <license>
'' Permission is hereby granted, free of charge, to any person obtaining a copy
'' of this software and associated documentation files (the "Software"), to
'' deal in the Software without restriction, including but without limitation
'' the rights to use, copy, modify, merge, publish, distribute, sublicense,
'' and/or sell copies of the Software, and to permit persons to whom the
'' Software is furnished to do so, subject to the following conditions:
''
'' The above copyright notice and this permission notice shall be included in
'' all copies or substantial portions of the Software.
''
'' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
'' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
'' IN THE SOFTWARE.
'' </license>
''-----------------------------------------------------------------------------
''
'' Copied from: http://www.michaelfcollins3.me/blog/2013/01/23/semantic_versioning_dotnet.html
''      github: https://gist.github.com/mfcollins3/4624831
'' Adapted to VB.net by ankostis@gmail.com


Imports System
Imports System.Diagnostics.CodeAnalysis
Imports System.Globalization
Imports System.Text.RegularExpressions
'Imports System.Diagnostics.Contracts

''' <summary>
''' Stores a semantic version number for a program.
''' </summary>
<Serializable()>
Public NotInheritable Class cSemanticVersion
    Implements IComparable, IComparable(Of cSemanticVersion), IEquatable(Of cSemanticVersion)

    ''' <summary>
    ''' A regular expression to detect whether a string contains only
    ''' digits.
    ''' </summary>
    Private Shared ReadOnly AllDigitsRegex As New Regex("^[0-9]+$", RegexOptions.Compiled Or RegexOptions.Singleline)

    ''' <summary>
    ''' A regular expression to detect whether a string contains only
    ''' digits.
    ''' </summary>
    Private Shared ReadOnly AlphaRegex As New Regex("^[A-Za-z0-9\-\.]+$", RegexOptions.Compiled Or RegexOptions.Singleline)

    ''' <summary>
    ''' The regular expression to use to parse a "strict" semantic version number.
    ''' </summary>
    Private Shared ReadOnly SemanticVersionRegex As New Regex( _
            "^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<prerelease>[A-Za-z0-9\-\.]+))?(\+(?<build>[A-Za-z0-9\-\.]+))?$", _
            RegexOptions.Compiled Or RegexOptions.Singleline)

    ' ''' <summary>
    ' ''' A utility regex used to parse loose version-strings to semantic-versions.
    ' ''' </summary>
    ' ''' <remarks>See http://search.cpan.org/dist/SemVer/lib/SemVer.pm</remarks>
    'Private Shared ReadOnly regex_1stPart As New Regex( _
    '        "^\s*(?:v(?:e(?:r(?:s(?:i(?:o(?:n)?)?)?)?)?)?)?\s*(?<major>\d+)(?:\.(?<nums>\d+))*(?<prerelease>[^+ ]+)?(\+(?<build>\S+))?", _
    '        RegexOptions.Compiled Or RegexOptions.Singleline, RegexOptions.IgnoreCase`)

    'Public Shared Function parse(ByVal version As String) As cSemanticVersion
    'End Function


    ''' <summary>
    ''' Initializes a new instance of the <see cref="cSemanticVersion"/> class.
    ''' </summary>
    ''' <param name="version">
    ''' The semantic version number to be parsed.
    ''' </param>
    Public Sub New(ByVal version As String)
        'Contract.Requires(Of ArgumentException)(Not String.IsNullOrEmpty(version))
        'Contract.Ensures(0 <= Me.MajorVersion)
        'Contract.Ensures(0 <= Me.MinorVersion)
        'Contract.Ensures(0 <= Me.PatchVersion)
        'Contract.Ensures(AlphaRegex.Match(Me.PrereleaseVersion).Success)
        'Contract.Ensures(AlphaRegex.Match(Me.BuildVersion).Success)

        Dim match = SemanticVersionRegex.Match(version)
        If (Not match.Success) Then
            Throw New ArgumentException(String.Format("Invalid Version-string({0})!", version))
        End If

        Me.MajorVersion = Integer.Parse(match.Groups("major").Value, CultureInfo.InvariantCulture)
        Me.MinorVersion = Integer.Parse(match.Groups("minor").Value, CultureInfo.InvariantCulture)
        Me.PatchVersion = Integer.Parse(match.Groups("patch").Value, CultureInfo.InvariantCulture)
        If match.Groups("prerelease").Success Then
            Me.PrereleaseVersion = match.Groups("prerelease").Value
        End If
        If match.Groups("build").Success Then
            Me.BuildVersion = match.Groups("build").Value
        End If
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="cSemanticVersion"/> class.
    ''' </summary>
    ''' <param name="majorVersion">
    ''' The major version number.
    ''' </param>
    ''' <param name="minorVersion">
    ''' The minor version number.
    ''' </param>
    ''' <param name="patchVersion">
    ''' The patch version number.
    ''' </param>
    Public Sub New(ByVal majorVersion As Integer, ByVal minorVersion As Integer, ByVal patchVersion As Integer, Optional ByVal prereleaseVersion As String = Nothing, Optional ByVal buildVersion As String = Nothing)
        'Contract.Requires(Of ArgumentException)(0 <= majorVersion)
        'Contract.Requires(Of ArgumentException)(0 <= minorVersion)
        'Contract.Requires(Of ArgumentException)(0 <= patchVersion)
        'Contract.Ensures(0 <= Me.MajorVersion)
        'Contract.Ensures(0 <= Me.MinorVersion)
        'Contract.Ensures(0 <= Me.PatchVersion)

        Me.MajorVersion = majorVersion
        Me.MinorVersion = minorVersion
        Me.PatchVersion = patchVersion
        Me.PrereleaseVersion = prereleaseVersion
        Me.BuildVersion = buildVersion
    End Sub


    ''' <summary>
    ''' Gets the build number.
    ''' </summary>
    ''' <value>
    ''' The value of this property is a string containing the build
    ''' identifier for the version number.
    ''' </value>
    Public Property BuildVersion As String

    ''' <summary>
    ''' Gets the major version number.
    ''' </summary>
    ''' <value>
    ''' The value of this property is a non-negative integer for the major
    ''' version number.
    ''' </value>
    Public Property MajorVersion As Integer

    ''' <summary>
    ''' Gets the minor version number.
    ''' </summary>
    ''' <value>
    ''' The value of this property is a non-negative integer for the minor
    ''' version number.
    ''' </value>
    Public Property MinorVersion As Integer

    ''' <summary>
    ''' Gets the patch version number.
    ''' </summary>
    ''' <value>
    ''' The value of this property is a non-negative integer for the patch
    ''' version number.
    ''' </value>
    Public Property PatchVersion As Integer

    ''' <summary>
    ''' Gets the pre-release version component.
    ''' </summary>
    ''' <value>
    ''' The value of this property is a string containing the pre-release
    ''' identifier.
    ''' </value>
    Public Property PrereleaseVersion As String

    ''' <summary>
    ''' Compares two <see cref="cSemanticVersion"/> objects for equality.
    ''' </summary>
    ''' <param name="version">
    ''' The first <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <param name="other">
    ''' The second semantic version object to compare.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if the objects are equal, or <b>false</b> if the
    ''' objects are not equal.
    ''' </returns>
    Public Shared Operator =(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        If (version Is Nothing) Then Return other Is Nothing

        Return version.Equals(other)
    End Operator

    ''' <summary>
    ''' Compares two <see cref="cSemanticVersion"/> objects for equality.
    ''' </summary>
    ''' <param name="version">
    ''' The first <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <param name="other">
    ''' The second <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if the objects are not equal, or <b>false</b> if the
    ''' objects are equal.
    ''' </returns>
    Public Shared Operator <>(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        If (version Is Nothing) Then Return Not other Is Nothing

        Return Not version.Equals(other)
    End Operator

    ''' <summary>
    ''' Compares two <see cref="cSemanticVersion"/> objects to determine if
    ''' the first object logically precedes the second object.
    ''' </summary>
    ''' <param name="version">
    ''' The first <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <param name="other">
    ''' The second <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if <paramref name="version"/> precedes 
    ''' <paramref name="other"/>, otherwise <b>false</b>.
    ''' </returns>
    Public Shared Operator <(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)
        'Contract.Requires(Of ArgumentNullException)(other IsNot Nothing)

        Return 0 > version.CompareTo(other)
    End Operator
    Public Shared Operator <=(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)
        'Contract.Requires(Of ArgumentNullException)(other IsNot Nothing)

        Return 0 >= version.CompareTo(other)
    End Operator

    ''' <summary>
    ''' Compares two <see cref="cSemanticVersion"/> object to determine if
    ''' the first object logically precedes the second object.
    ''' </summary>
    ''' <param name="version">
    ''' The first <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <param name="other">
    ''' The second <see cref="cSemanticVersion"/> object to compare.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if <paramref name="version"/> follows
    ''' <paramref name="other"/>, otherwise <b>false</b>.
    ''' </returns>
    Public Shared Operator >(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)

        Return 0 < version.CompareTo(other)
    End Operator
    Public Shared Operator >=(ByVal version As cSemanticVersion, ByVal other As cSemanticVersion) As Boolean
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)
        'Contract.Requires(Of ArgumentNullException)(version IsNot Nothing)

        Return 0 <= version.CompareTo(other)
    End Operator

    ''' <summary>
    ''' Compares two objects.
    ''' </summary>
    ''' <param name="obj">
    ''' The object to compare to this object.
    ''' </param>
    ''' <returns>
    ''' Returns a value that indicates the relative order of the objects
    ''' that are being compared.
    ''' <list type="table">
    ''' <listheader>
    ''' <term>Value</term>
    ''' <description>Meaning</description>
    ''' </listheader>
    ''' <item>
    ''' <term>Less than zero</term>
    ''' <description>
    ''' This instance precedes <paramref name="obj"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Zero</term>
    ''' <description>
    ''' This instance occurs in the same position in the sort order as
    ''' <paramref name="obj"/>.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Greater than zero</term>
    ''' <description>
    ''' This instance follows <paramref name="obj"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' <paramref name="obj"/> is not a <see cref="cSemanticVersion"/>
    ''' object.
    ''' </exception>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
        If obj Is Nothing Then Throw New ArgumentNullException("Other Object is Null!")
        If Not TypeOf obj Is cSemanticVersion Then Throw New ArgumentException(String.Format("Other Object({0}) is not a SemanticVersion!", obj))

        Return Me.CompareTo(DirectCast(obj, cSemanticVersion))
    End Function

    ''' <summary>
    ''' Compares the current object with another 
    ''' <see cref="cSemanticVersion"/> object.
    ''' </summary>
    ''' <param name="other">
    ''' The other <see cref="cSemanticVersion"/> object to compare to this
    ''' instance.
    ''' </param>
    ''' <returns>
    ''' Returns a value that indicates the relative order of the objects
    ''' that are being compared.
    ''' <list type="table">
    ''' <listheader>
    ''' <term>Value</term>
    ''' <description>Meaning</description>
    ''' </listheader>
    ''' <item>
    ''' <term>Less than zero</term>
    ''' <description>
    ''' This instance precedes <paramref name="other"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Zero</term>
    ''' <description>
    ''' This instance occurs in the same position in the sort order as
    ''' <paramref name="other"/>.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Greater than zero</term>
    ''' <description>
    ''' This instance follows <paramref name="other"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </returns>
    Public Function CompareTo(ByVal other As cSemanticVersion) As Integer Implements IComparable(Of AirDrag.cSemanticVersion).CompareTo
        If (other Is Nothing) Then Throw New ArgumentNullException("other")

        If (other Is Me) Then Return 0

        Dim result = Me.MajorVersion.CompareTo(other.MajorVersion)
        If (0 = result) Then
            result = Me.MinorVersion.CompareTo(other.MinorVersion)
            If (0 = result) Then
                result = Me.PatchVersion.CompareTo(other.PatchVersion)
                If (0 = result) Then
                    result = ComparePrereleaseVersions(Me.PrereleaseVersion, other.PrereleaseVersion)
                End If
            End If
        End If

        Return result
    End Function

    ''' <summary>
    ''' Compares this instance to another object for equality.
    ''' </summary>
    ''' <param name="obj">
    ''' The object to compare to this instance.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if the objects are equal, or <b>false</b> if the
    ''' objects are not equal.
    ''' </returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If (obj Is Nothing) Then Return False
        If (obj Is Me) Then Return True
        If (Not TypeOf obj Is cSemanticVersion) Then Return False

        Return Me.Equals(DirectCast(obj, cSemanticVersion))
    End Function

    ''' <summary>
    ''' Compares this instance to another <see cref="cSemanticVersion"/>
    ''' object for equality.
    ''' </summary>
    ''' <param name="other">
    ''' The <see cref="cSemanticVersion"/> object to compare to this
    ''' instance.
    ''' </param>
    ''' <returns>
    ''' <b>True</b> if the objects are equal, or false if the objects are
    ''' not equal.
    ''' </returns>
    Public Overloads Function Equals(ByVal other As cSemanticVersion) As Boolean Implements IEquatable(Of AirDrag.cSemanticVersion).Equals
        If (other Is Me) Then Return True

        If (other Is Nothing) Then Return False

        Return Me.MajorVersion = other.MajorVersion AndAlso Me.MinorVersion = other.MinorVersion _
                   AndAlso Me.PatchVersion = other.PatchVersion AndAlso Me.PrereleaseVersion = other.PrereleaseVersion _
                   AndAlso Me.BuildVersion = other.BuildVersion
    End Function


    ''' <summary>
    ''' Calculates the hash code for the object.
    ''' </summary>
    ''' <returns>
    ''' The hash code for the object.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer
        Dim hashCode = 17
        hashCode = (hashCode * 37) + Me.MajorVersion
        hashCode = (hashCode * 37) + Me.MinorVersion
        hashCode = (hashCode * 37) + Me.PatchVersion
        If Me.PrereleaseVersion IsNot Nothing Then
            hashCode = (hashCode * 37) + Me.PrereleaseVersion.GetHashCode()
        End If

        If Me.BuildVersion IsNot Nothing Then
            hashCode = (hashCode * 37) + Me.BuildVersion.GetHashCode()
        End If
        Return hashCode
    End Function

    ''' <summary>
    ''' Returns the string representation of the semantic version number.
    ''' </summary>
    ''' <returns>
    ''' The semantic version number.
    ''' </returns>
    Public Overrides Function ToString() As String
        Return String.Format(
            CultureInfo.InvariantCulture,
            "{0}.{1}.{2}{3}{4}",
            Me.MajorVersion,
            Me.MinorVersion,
            Me.PatchVersion,
            IIf(String.IsNullOrEmpty(Me.PrereleaseVersion), String.Empty, "-" + Me.PrereleaseVersion),
            IIf(String.IsNullOrEmpty(Me.BuildVersion), String.Empty, "+" + Me.BuildVersion))
    End Function

    ''' <summary>
    ''' Compares two build version values to determine precedence.
    ''' </summary>
    ''' <param name="identifier1">
    ''' The first identifier to compare.
    ''' </param>
    ''' <param name="identifier2">
    ''' The second identifier to compare.
    ''' </param>
    ''' <returns>
    ''' Returns a value that indicates the relative order of the objects
    ''' that are being compared.
    ''' <list type="table">
    ''' <listheader>
    ''' <term>Value</term>
    ''' <description>Meaning</description>
    ''' </listheader>
    ''' <item>
    ''' <term>Less than zero</term>
    ''' <description>
    ''' <paramref name="identifier1"/> precedes 
    ''' <paramref name="identifier2"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Zero</term>
    ''' <description>
    ''' The identifiers occur in the same position in the sort order.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>Greater than zero</term>
    ''' <description>
    ''' <paramref name="identifier1"/> follows 
    ''' <paramref name="identifier2"/> in the sort order.
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </returns>
    Private Shared Function ComparePrereleaseVersions(ByVal identifier1 As String, ByVal identifier2 As String) As Integer
        Dim result As Integer = 0
        Dim hasIdentifier1 = Not String.IsNullOrEmpty(identifier1)
        Dim hasIdentifier2 = Not String.IsNullOrEmpty(identifier2)
        If (hasIdentifier1 AndAlso Not hasIdentifier2) Then
            result = -1
        ElseIf (Not hasIdentifier1 AndAlso hasIdentifier2) Then
            result = 1
        ElseIf (hasIdentifier1) Then
            Dim dotDelimiter As Char() = {"."c}
            Dim parts1 = identifier1.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries)
            Dim parts2 = identifier2.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries)
            Dim max = Math.Max(parts1.Length, parts2.Length)
            For i = 0 To max
                If (i = parts1.Length AndAlso i <> parts2.Length) Then
                    result = -1
                    Exit For
                End If

                If (i <> parts1.Length AndAlso i = parts2.Length) Then
                    result = 1
                    Exit For
                End If

                Dim part1 = parts1(i)
                Dim part2 = parts2(i)
                If (AllDigitsRegex.IsMatch(part1) AndAlso AllDigitsRegex.IsMatch(part2)) Then
                    Dim value1 = Integer.Parse(part1, CultureInfo.InvariantCulture)
                    Dim value2 = Integer.Parse(part2, CultureInfo.InvariantCulture)
                    result = value1.CompareTo(value2)
                Else
                    result = String.Compare(part1, part2, StringComparison.Ordinal)

                    If (0 <> result) Then
                        Exit For
                    End If
                End If
            Next
        End If

        Return result
    End Function

End Class
