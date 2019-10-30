/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest.repr
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;

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
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.FirstItemIterable<Representation> results = new org.Neo4Net.helpers.collection.FirstItemIterable<>(new org.Neo4Net.helpers.collection.IteratorWrapper<Representation,Object>(data)
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
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.FirstItemIterable<Representation> results = convertValuesToRepresentations(data);
			  FirstItemIterable<Representation> results = ConvertValuesToRepresentations( data );
			  return new ServerListRepresentation( GetType( results ), results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static org.Neo4Net.helpers.collection.FirstItemIterable<Representation> convertValuesToRepresentations(Iterable data)
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
			  else if ( result is IGraphDatabaseService )
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