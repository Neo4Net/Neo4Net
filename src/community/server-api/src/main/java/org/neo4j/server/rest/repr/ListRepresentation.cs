using System.Collections.Generic;

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

	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;

	public class ListRepresentation : Representation
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected final Iterable<? extends Representation> content;
		 protected internal readonly IEnumerable<Representation> Content;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ListRepresentation(final String type, final Iterable<? extends Representation> content)
		 public ListRepresentation<T1>( string type, IEnumerable<T1> content ) where T1 : Representation : base( type )
		 {
			  this.Content = content;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ListRepresentation(RepresentationType type, final Iterable<? extends Representation> content)
		 public ListRepresentation<T1>( RepresentationType type, IEnumerable<T1> content ) where T1 : Representation : base( type )
		 {
			  this.Content = content;
		 }

		 internal override string Serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
		 {
			  ListWriter writer = format.SerializeList( Type );
			  Serialize( new ListSerializer( writer, baseUri, extensions ) );
			  writer.Done();
			  return format.Complete( writer );
		 }

		 internal virtual void Serialize( ListSerializer serializer )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends Representation> contentIterator = content.iterator();
			  IEnumerator<Representation> contentIterator = Content.GetEnumerator();

			  try
			  {
					while ( contentIterator.MoveNext() )
					{
						 Representation repr = contentIterator.Current;
						 repr.AddTo( serializer );
					}
			  }
			  finally
			  {
					// Make sure we exhaust this iterator in case it has an internal close mechanism
					while ( contentIterator.MoveNext() )
					{
						 contentIterator.Current;
					}
			  }
		 }

		 internal override void AddTo( ListSerializer serializer )
		 {
			  serializer.AddList( this );
		 }

		 internal override void PutTo( MappingSerializer serializer, string key )
		 {
			  serializer.PutList( key, this );
		 }

		 public static ListRepresentation Strings( params string[] values )
		 {
			  return String( Arrays.asList( values ) );
		 }

		 public static ListRepresentation String( IEnumerable<string> values )
		 {
			  return new ListRepresentation( RepresentationType.String, new IterableWrapperAnonymousInnerClass( values ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, string>
		 {
			 public IterableWrapperAnonymousInnerClass( IEnumerable<string> values ) : base( values )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( string value )
			 {
				  return ValueRepresentation.String( value );
			 }
		 }

		 public static ListRepresentation Points( params Point[] values )
		 {
			  return Point( Arrays.asList( values ) );
		 }

		 public static ListRepresentation Point( IEnumerable<Point> values )
		 {
			  return new ListRepresentation( RepresentationType.Point, new IterableWrapperAnonymousInnerClass2( values ) );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<Representation, Point>
		 {
			 public IterableWrapperAnonymousInnerClass2( IEnumerable<Point> values ) : base( values )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( Point value )
			 {
				  return ValueRepresentation.Point( value );
			 }
		 }

		 public static ListRepresentation Temporals( params Temporal[] values )
		 {
			  return Temporal( Arrays.asList( values ) );
		 }

		 public static ListRepresentation Temporal( IEnumerable<Temporal> values )
		 {
			  return new ListRepresentation( RepresentationType.Temporal, new IterableWrapperAnonymousInnerClass3( values ) );
		 }

		 private class IterableWrapperAnonymousInnerClass3 : IterableWrapper<Representation, Temporal>
		 {
			 public IterableWrapperAnonymousInnerClass3( IEnumerable<Temporal> values ) : base( values )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( Temporal value )
			 {
				  return ValueRepresentation.Temporal( value );
			 }
		 }

		 public static ListRepresentation TemporalAmounts( params TemporalAmount[] values )
		 {
			  return TemporalAmount( Arrays.asList( values ) );
		 }

		 public static ListRepresentation TemporalAmount( IEnumerable<TemporalAmount> values )
		 {
			  return new ListRepresentation( RepresentationType.TemporalAmount, new IterableWrapperAnonymousInnerClass4( values ) );
		 }

		 private class IterableWrapperAnonymousInnerClass4 : IterableWrapper<Representation, TemporalAmount>
		 {
			 public IterableWrapperAnonymousInnerClass4( IEnumerable<TemporalAmount> values ) : base( values )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( TemporalAmount value )
			 {
				  return ValueRepresentation.TemporalAmount( value );
			 }
		 }

		 public static ListRepresentation RelationshipTypes( IEnumerable<RelationshipType> types )
		 {
			  return new ListRepresentation( RepresentationType.RelationshipType, new IterableWrapperAnonymousInnerClass5( types ) );
		 }

		 private class IterableWrapperAnonymousInnerClass5 : IterableWrapper<Representation, RelationshipType>
		 {
			 public IterableWrapperAnonymousInnerClass5( IEnumerable<RelationshipType> types ) : base( types )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( RelationshipType value )
			 {
				  return ValueRepresentation.RelationshipType( value );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ListRepresentation numbers(final long... values)
		 public static ListRepresentation Numbers( params long[] values )
		 {
			  return new ListRepresentation( RepresentationType.Long, ( IEnumerable<ValueRepresentation> )() => new PrefetchingIteratorAnonymousInnerClass(values) );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<ValueRepresentation>
		 {
			 private long[] _values;

			 public PrefetchingIteratorAnonymousInnerClass( long[] values )
			 {
				 this._values = values;
			 }

			 internal int pos;

			 protected internal override ValueRepresentation fetchNextOrNull()
			 {
				  if ( pos >= _values.Length )
				  {
						return null;
				  }
				  return ValueRepresentation.Number( _values[pos++] );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ListRepresentation numbers(final double[] values)
		 public static ListRepresentation Numbers( double[] values )
		 {
			  return new ListRepresentation( RepresentationType.Double, ( IEnumerable<ValueRepresentation> )() => new PrefetchingIteratorAnonymousInnerClass2(values) );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass2 : PrefetchingIterator<ValueRepresentation>
		 {
			 private double[] _values;

			 public PrefetchingIteratorAnonymousInnerClass2( double[] values )
			 {
				 this._values = values;
			 }

			 internal int pos;

			 protected internal override ValueRepresentation fetchNextOrNull()
			 {
				  if ( pos >= _values.Length )
				  {
						return null;
				  }
				  return ValueRepresentation.Number( _values[pos++] );
			 }
		 }
	}

}