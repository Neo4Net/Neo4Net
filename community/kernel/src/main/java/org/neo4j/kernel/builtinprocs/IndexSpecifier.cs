using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.builtinprocs
{

	public class IndexSpecifier
	{
		 private const string GROUP_INDEX_NAME = "INAME";
		 private const string GROUP_QUOTED_INDEX_NAME = "QINAME";
		 private const string GROUP_LABEL = "LABEL";
		 private const string GROUP_QUOTED_LABEL = "QLABEL";
		 private const string GROUP_PROPERTY = "PROPERTY";
		 private const string GROUP_QUOTED_PROPERTY = "QPROPERTY";

		 private static readonly string _whitespace = ZeroOrMore( "\\s" );
		 private static readonly string _initiator = "\\A" + _whitespace; // Matches beginning of input, and optional whitespace.
		 private static readonly string _terminator = _whitespace + "\\z"; // Matches optional whitespace, and end of input.
		 private static readonly string @continue = "\\G" + _whitespace; // Matches end-boundary of previous match, and optional whitespace.

		 private static readonly string _indexName = Or( Identifier( GROUP_INDEX_NAME ), QuotedIdentifier( GROUP_QUOTED_INDEX_NAME ) );
		 private static readonly string _label = ":" + _whitespace + Or( Identifier( GROUP_LABEL ), QuotedIdentifier( GROUP_QUOTED_LABEL ) );
		 private static readonly string _indexOrLabel = Or( _indexName, _label );
		 private const string PROPERTY_CLAUSE_BEGIN = "\\(";
		 private static readonly string _property = Or( Identifier( GROUP_PROPERTY ), QuotedIdentifier( GROUP_QUOTED_PROPERTY ) );
		 private static readonly string _firstProperty = _property;
		 private static readonly string _followingProperty = "," + _whitespace + _property;
		 private const string PROPERTY_CLAUSE_END = "\\)";

		 private static readonly Pattern _patternStartIndexNameOrLabel = Pattern.compile( _initiator + _indexOrLabel ); // Initiating pattern.
		 private static readonly Pattern _patternIndexNameEnd = Pattern.compile( @continue + _terminator ); // Terminating pattern.
		 private static readonly Pattern _patternPropertyClauseBegin = Pattern.compile( @continue + PROPERTY_CLAUSE_BEGIN );
		 private static readonly Pattern _patternFirstProperty = Pattern.compile( @continue + _firstProperty );
		 private static readonly Pattern _patternFollowingProperty = Pattern.compile( @continue + _followingProperty );
		 private static readonly Pattern _patternPropertyClauseEnd = Pattern.compile( @continue + PROPERTY_CLAUSE_END + _terminator ); // Terminating pattern.

		 private readonly string _specification;
		 private readonly string _label;
		 private readonly string[] _properties;
		 private readonly string _name;

		 public static IndexSpecifier ByPatternOrName( string specification )
		 {
			  return Parse( specification, true, true );
		 }

		 public static IndexSpecifier ByPattern( string specification )
		 {
			  return Parse( specification, false, true );
		 }

		 public static IndexSpecifier ByName( string specification )
		 {
			  return Parse( specification, true, false );
		 }

		 private static IndexSpecifier Parse( string specification, bool allowIndexNameSpecs, bool allowIndexPatternSpecs )
		 {
			  Matcher matcher = _patternStartIndexNameOrLabel.matcher( specification );
			  if ( !matcher.find() )
			  {
					throw new System.ArgumentException( "Cannot parse index specification: '" + specification + "'" );
			  }

			  string indexName = Either( matcher.group( GROUP_INDEX_NAME ), matcher.group( GROUP_QUOTED_INDEX_NAME ) );
			  if ( !string.ReferenceEquals( indexName, null ) )
			  {
					if ( !allowIndexNameSpecs )
					{
						 throw new System.ArgumentException( "Cannot parse index specification: '" + specification + "' - it looks like an index name, which is not allowed." );
					}
					matcher.usePattern( _patternIndexNameEnd );
					if ( matcher.find() )
					{
						 return new IndexSpecifier( specification, indexName );
					}
					throw new System.ArgumentException( "Invalid characters following index name: '" + specification + "'" );
			  }

			  if ( !allowIndexPatternSpecs )
			  {
					throw new System.ArgumentException( "Cannot parse index specification: '" + specification + "' - it looks like an index pattern, but an index name was expected." );
			  }

			  string label = Either( matcher.group( GROUP_LABEL ), matcher.group( GROUP_QUOTED_LABEL ) );
			  if ( string.ReferenceEquals( label, null ) )
			  {
					throw new System.ArgumentException( "Cannot parse index specification: '" + specification + "'" );
			  }

			  matcher.usePattern( _patternPropertyClauseBegin );
			  if ( !matcher.find() )
			  {
					throw new System.ArgumentException( "Expected to find a property clause following the label: '" + specification + "'" );
			  }

			  matcher.usePattern( _patternFirstProperty );
			  if ( !matcher.find() )
			  {
					throw new System.ArgumentException( "Expected to find a property in the property clause following the label: '" + specification + "'" );
			  }

			  IList<string> properties = new List<string>();
			  do
			  {
					string property = Either( matcher.group( GROUP_PROPERTY ), matcher.group( GROUP_QUOTED_PROPERTY ) );
					if ( string.ReferenceEquals( property, null ) )
					{
						 throw new System.ArgumentException( "Expected to find a property in the property clause following the label: '" + specification + "'" );
					}
					properties.Add( property );
					if ( matcher.pattern() != _patternFollowingProperty )
					{
						 matcher.usePattern( _patternFollowingProperty );
					}
			  } while ( matcher.find() );

			  matcher.usePattern( _patternPropertyClauseEnd );
			  if ( !matcher.find() )
			  {
					throw new System.ArgumentException( "The property clause is not terminated: '" + specification + "'" );
			  }

			  return new IndexSpecifier( specification, label, properties.ToArray() );
		 }

		 private static string Either( string first, string second )
		 {
			  return !string.ReferenceEquals( first, null ) ? first : second;
		 }

		 private static string Or( string first, string second )
		 {
			  return Group( Group( first ) + "|" + Group( second ) );
		 }

		 private static string Identifier( string name )
		 {
			  return Capture( name, "[A-Za-z0-9_]+" );
		 }

		 private static string QuotedIdentifier( string name )
		 {
			  return Group( "`" + Capture( name, OneOrMore( Group( Or( "[^`]", "``" ) ) ) ) + "`" );
		 }

		 private static string Group( string contents )
		 {
			  return "(?:" + contents + ")";
		 }

		 private static string Capture( string name, string contents )
		 {
			  return "(?<" + name + ">" + contents + ")";
		 }

		 private static string ZeroOrMore( string terms )
		 {
			  return terms + "*";
		 }

		 private static string OneOrMore( string terms )
		 {
			  return terms + "+";
		 }

		 private IndexSpecifier( string specification, string indexName )
		 {
			  this._specification = specification;
			  this._label = null;
			  this._properties = null;
			  this._name = indexName;
		 }

		 private IndexSpecifier( string specification, string label, string[] properties )
		 {
			  this._specification = specification;
			  this._label = label;
			  this._properties = properties;
			  this._name = null;
		 }

		 public virtual string Label()
		 {
			  return _label;
		 }

		 public virtual string[] Properties()
		 {
			  return _properties;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public override string ToString()
		 {
			  return _specification;
		 }
	}

}