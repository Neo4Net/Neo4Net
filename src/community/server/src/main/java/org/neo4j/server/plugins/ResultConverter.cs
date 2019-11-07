using System;
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
namespace Neo4Net.Server.plugins
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.Collections.Helpers;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using NodeRepresentation = Neo4Net.Server.rest.repr.NodeRepresentation;
	using Neo4Net.Server.rest.repr;
	using RelationshipRepresentation = Neo4Net.Server.rest.repr.RelationshipRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationType = Neo4Net.Server.rest.repr.RepresentationType;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

	internal abstract class ResultConverter
	{
		 internal static ResultConverter Get( Type type )
		 {
			  return Get( type, true );
		 }

		 private static ResultConverter Get( Type type, bool allowComplex )
		 {
			  if ( type is Type )
			  {
					Type cls = ( Type ) type;
					if ( allowComplex && cls.IsAssignableFrom( typeof( Representation ) ) )
					{
						 return IDENTITY_RESULT;
					}
					else if ( cls == typeof( Node ) )
					{
						 return NODE_RESULT;
					}
					else if ( cls == typeof( Relationship ) )
					{
						 return RELATIONSHIP_RESULT;
					}
					else if ( cls == typeof( Path ) )
					{
						 return PATH_RESULT;
					}
					else if ( cls == typeof( string ) )
					{
						 return STRING_RESULT;
					}
					else if ( cls == typeof( void ) || cls == typeof( Void ) )
					{
						 return VOID_RESULT;
					}
					else if ( cls == typeof( long ) || cls == typeof( Long ) )
					{
						 return LONG_RESULT;
					}
					else if ( cls == typeof( double ) || cls == typeof( float ) || cls == typeof( Double ) || cls == typeof( Float ) )
					{
						 return DOUBLE_RESULT;
					}
					else if ( cls == typeof( bool ) || cls == typeof( Boolean ) )
					{
						 return BOOL_RESULT;
					}
					else if ( cls == typeof( char ) || cls == typeof( Character ) )
					{
						 return CHAR_RESULT;
					}
					else if ( cls.IsPrimitive || ( cls.IsAssignableFrom( typeof( Number ) ) && cls.Assembly.GetName().Name.Equals("java.lang") ) )
					{
						 return INT_RESULT;
					}
			  }
			  else if ( allowComplex && type is ParameterizedType )
			  {
					ParameterizedType parameterizedType = ( ParameterizedType ) type;
					Type raw = ( Type ) parameterizedType.RawType;
					Type paramType = parameterizedType.ActualTypeArguments[0];
					if ( !( paramType is Type ) )
					{
						 throw new System.InvalidOperationException( "Parameterized result types must have a concrete type parameter." );
					}
					Type param = ( Type ) paramType;
					if ( raw.IsAssignableFrom( typeof( System.Collections.IEnumerable ) ) )
					{
						 return new ListResult( Get( param, false ) );
					}
			  }
			  throw new System.InvalidOperationException( "Illegal result type: " + type );
		 }

		 internal abstract Representation Convert( object obj );

		 internal abstract RepresentationType Type();

		 private abstract class ValueResult : ResultConverter
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RepresentationType TypeConflict;

			  internal ValueResult( RepresentationType type )
			  {
					this.TypeConflict = type;
			  }

			  internal override RepresentationType Type()
			  {
					return TypeConflict;
			  }
		 }

		 private static readonly ResultConverter IDENTITY_RESULT = new ResultConverterAnonymousInnerClass();

		 private class ResultConverterAnonymousInnerClass : ResultConverter
		 {
			 internal override Representation convert( object obj )
			 {
				  return ( Representation ) obj;
			 }

			 internal override RepresentationType type()
			 {
				  return null;
			 }
		 }
		 private static readonly ResultConverter NODE_RESULT = new ValueResultAnonymousInnerClass( RepresentationType.NODE );

		 private class ValueResultAnonymousInnerClass : ValueResult
		 {
			 public ValueResultAnonymousInnerClass( RepresentationType node ) : base( node )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return new NodeRepresentation( ( Node ) obj );
			 }
		 }
		 private static readonly ResultConverter RELATIONSHIP_RESULT = new ValueResultAnonymousInnerClass2( RepresentationType.RELATIONSHIP );

		 private class ValueResultAnonymousInnerClass2 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass2( RepresentationType relationship ) : base( relationship )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return new RelationshipRepresentation( ( Relationship ) obj );
			 }
		 }
		 private static readonly ResultConverter PATH_RESULT = new ValueResultAnonymousInnerClass3( RepresentationType.PATH );

		 private class ValueResultAnonymousInnerClass3 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass3( RepresentationType path ) : base( path )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return new PathRepresentation<>( ( Path ) obj );
			 }
		 }
		 private static readonly ResultConverter STRING_RESULT = new ValueResultAnonymousInnerClass4( RepresentationType.STRING );

		 private class ValueResultAnonymousInnerClass4 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass4( RepresentationType @string ) : base( @string )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.@string( ( string ) obj );
			 }
		 }
		 private static readonly ResultConverter LONG_RESULT = new ValueResultAnonymousInnerClass5( RepresentationType.LONG );

		 private class ValueResultAnonymousInnerClass5 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass5( RepresentationType @long ) : base( @long )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.number( ( ( Number ) obj ).longValue() );
			 }
		 }
		 private static readonly ResultConverter DOUBLE_RESULT = new ValueResultAnonymousInnerClass6( RepresentationType.DOUBLE );

		 private class ValueResultAnonymousInnerClass6 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass6( RepresentationType @double ) : base( @double )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.number( ( ( Number ) obj ).doubleValue() );
			 }
		 }
		 private static readonly ResultConverter BOOL_RESULT = new ValueResultAnonymousInnerClass7( RepresentationType.BOOLEAN );

		 private class ValueResultAnonymousInnerClass7 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass7( RepresentationType boolean ) : base( boolean )
			 {
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") Neo4Net.server.rest.repr.Representation convert(Object obj)
			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.@bool( ( bool? ) obj.Value );
			 }
		 }
		 private static readonly ResultConverter INT_RESULT = new ValueResultAnonymousInnerClass8( RepresentationType.INTEGER );

		 private class ValueResultAnonymousInnerClass8 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass8( RepresentationType integer ) : base( integer )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.number( ( ( Number ) obj ).intValue() );
			 }
		 }
		 private static readonly ResultConverter CHAR_RESULT = new ValueResultAnonymousInnerClass9( RepresentationType.CHAR );

		 private class ValueResultAnonymousInnerClass9 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass9( RepresentationType @char ) : base( @char )
			 {
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") Neo4Net.server.rest.repr.Representation convert(Object obj)
			 internal override Representation convert( object obj )
			 {
				  return ValueRepresentation.number( ( char? ) obj );
			 }
		 }
		 private static readonly ResultConverter VOID_RESULT = new ValueResultAnonymousInnerClass10( RepresentationType.NOTHING );

		 private class ValueResultAnonymousInnerClass10 : ValueResult
		 {
			 public ValueResultAnonymousInnerClass10( RepresentationType nothing ) : base( nothing )
			 {
			 }

			 internal override Representation convert( object obj )
			 {
				  return Representation.emptyRepresentation();
			 }
		 }

		 private class ListResult : ResultConverter
		 {
			  internal readonly ResultConverter ItemConverter;

			  internal ListResult( ResultConverter itemConverter )
			  {
					this.ItemConverter = itemConverter;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") Neo4Net.server.rest.repr.Representation convert(Object obj)
			  internal override Representation Convert( object obj )
			  {
					return new ListRepresentation( ItemConverter.type(), new IterableWrapperAnonymousInnerClass(this) );
			  }

			  private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, object>
			  {
				  private readonly ListResult _outerInstance;

				  public IterableWrapperAnonymousInnerClass( ListResult outerInstance ) : base( ( IEnumerable<object> ) obj )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override Representation underlyingObjectToObject( object @object )
				  {
						return _outerInstance.itemConverter.convert( @object );
				  }
			  }

			  internal override RepresentationType Type()
			  {
					return null;
			  }
		 }
	}

}