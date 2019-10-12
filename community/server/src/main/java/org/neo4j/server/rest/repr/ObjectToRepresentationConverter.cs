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
namespace Org.Neo4j.Server.rest.repr
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;

	public class ObjectToRepresentationConverter
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Representation convert(final Object data)
		 public static Representation Convert( object data )
		 {
			  if ( data is System.Collections.IEnumerable )
			  {
					return GetListRepresentation( ( System.Collections.IEnumerable ) data );
			  }
			  if ( data is System.Collections.IEnumerator )
			  {
					System.Collections.IEnumerator iterator = ( System.Collections.IEnumerator ) data;
					return GetIteratorRepresentation( iterator );
			  }
			  if ( data is System.Collections.IDictionary )
			  {

					return GetMapRepresentation( ( System.Collections.IDictionary ) data );
			  }
			  return GetSingleRepresentation( data );
		 }

		 private ObjectToRepresentationConverter()
		 {
		 }

		 public static MappingRepresentation GetMapRepresentation( System.Collections.IDictionary data )
		 {

			  return new MapRepresentation( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static Representation getIteratorRepresentation(java.util.Iterator data)
		 internal static Representation GetIteratorRepresentation( System.Collections.IEnumerator data )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.FirstItemIterable<Representation> results = new org.neo4j.helpers.collection.FirstItemIterable<>(new org.neo4j.helpers.collection.IteratorWrapper<Representation,Object>(data)
			  FirstItemIterable<Representation> results = new FirstItemIterable<Representation>( new IteratorWrapperAnonymousInnerClass( data ) );
			  return new ListRepresentation( GetType( results ), results );
		 }

		 private class IteratorWrapperAnonymousInnerClass : IteratorWrapper<Representation, object>
		 {
			 public IteratorWrapperAnonymousInnerClass( System.Collections.IEnumerator data ) : base( data )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( object value )
			 {
				  if ( value is System.Collections.IEnumerable )
				  {
						FirstItemIterable<Representation> nested = ConvertValuesToRepresentations( ( System.Collections.IEnumerable ) value );
						return new ListRepresentation( GetType( nested ), nested );
				  }
				  else
				  {
						return GetSingleRepresentation( value );
				  }
			 }
		 }

		 public static ListRepresentation GetListRepresentation( System.Collections.IEnumerable data )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.FirstItemIterable<Representation> results = convertValuesToRepresentations(data);
			  FirstItemIterable<Representation> results = ConvertValuesToRepresentations( data );
			  return new ServerListRepresentation( GetType( results ), results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static org.neo4j.helpers.collection.FirstItemIterable<Representation> convertValuesToRepresentations(Iterable data)
		 internal static FirstItemIterable<Representation> ConvertValuesToRepresentations( System.Collections.IEnumerable data )
		 {
			  return new FirstItemIterable<Representation>( new IterableWrapperAnonymousInnerClass( data ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, object>
		 {
			 public IterableWrapperAnonymousInnerClass( System.Collections.IEnumerable data ) : base( data )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( object value )
			 {
				  return Convert( value );
			 }
		 }

		 internal static RepresentationType GetType( FirstItemIterable<Representation> representations )
		 {
			  Representation representation = representations.First;
			  if ( representation == null )
			  {
					return RepresentationType.String;
			  }
			  return representation.RepresentationType;
		 }

		 internal static Representation GetSingleRepresentation( object result )
		 {
			  if ( result == null )
			  {
					return ValueRepresentation.OfNull();
			  }
			  else if ( result is GraphDatabaseService )
			  {
					return new DatabaseRepresentation();
			  }
			  else if ( result is Node )
			  {
					return new NodeRepresentation( ( Node ) result );
			  }
			  else if ( result is Relationship )
			  {
					return new RelationshipRepresentation( ( Relationship ) result );
			  }
			  else if ( result is double? || result is float? )
			  {
					return ValueRepresentation.number( ( ( Number ) result ).doubleValue() );
			  }
			  else if ( result is long? )
			  {
					return ValueRepresentation.number( ( ( long? ) result ).Value );
			  }
			  else if ( result is int? )
			  {
					return ValueRepresentation.number( ( ( int? ) result ).Value );
			  }
			  else if ( result is bool? )
			  {
					return ValueRepresentation.Bool( ( ( bool? ) result ).Value );
			  }
			  else
			  {
					return ValueRepresentation.String( result.ToString() );
			  }
		 }
	}

}