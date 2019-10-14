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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	/// <summary>
	/// Simple utility for gathering all information about an <seealso cref="InputEntityVisitor"/> and exposing getters
	/// for that data. Easier to work with than purely visitor-based implementation in tests.
	/// </summary>
	public class InputEntity : InputEntityVisitor, ICloneable
	{
		 public static readonly object[] NoProperties = new object[0];
		 public static readonly string[] NoLabels = new string[0];

		 private readonly InputEntityVisitor @delegate;

		 public InputEntity( InputEntityVisitor @delegate )
		 {
			  this.@delegate = @delegate;
			  Clear();
		 }

		 public InputEntity() : this(InputEntityVisitor.NULL)
		 {
		 }

		 public bool HasPropertyId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long PropertyIdConflict;
		 public bool HasIntPropertyKeyIds;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly IList<object> PropertiesConflict = new List<object>();

		 public bool HasLongId;
		 public long LongId;
		 public object ObjectId;
		 public Group IdGroup;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly IList<string> LabelsConflict = new List<string>();
		 public bool HasLabelField;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long LabelFieldConflict;

		 public bool HasLongStartId;
		 public long LongStartId;
		 public object ObjectStartId;
		 public Group StartIdGroup;

		 public bool HasLongEndId;
		 public long LongEndId;
		 public object ObjectEndId;
		 public Group EndIdGroup;

		 public bool HasIntType;
		 public int IntType;
		 public string StringType;

		 private bool _end;

		 public override bool PropertyId( long nextProp )
		 {
			  CheckClear();
			  HasPropertyId = true;
			  PropertyIdConflict = nextProp;
			  return @delegate.PropertyId( nextProp );
		 }

		 public override bool Property( string key, object value )
		 {
			  CheckClear();
			  PropertiesConflict.Add( key );
			  PropertiesConflict.Add( value );
			  return @delegate.Property( key, value );
		 }

		 public override bool Property( int propertyKeyId, object value )
		 {
			  CheckClear();
			  HasIntPropertyKeyIds = true;
			  PropertiesConflict.Add( propertyKeyId );
			  PropertiesConflict.Add( value );
			  return @delegate.Property( propertyKeyId, value );
		 }

		 public override bool Id( long id )
		 {
			  CheckClear();
			  HasLongId = true;
			  LongId = id;
			  return @delegate.Id( id );
		 }

		 public override bool Id( object id, Group group )
		 {
			  CheckClear();
			  ObjectId = id;
			  IdGroup = group;
			  return @delegate.Id( id, group );
		 }

		 public override bool Labels( string[] labels )
		 {
			  CheckClear();
			  Collections.addAll( this.LabelsConflict, labels );
			  return @delegate.Labels( labels );
		 }

		 public override bool LabelField( long labelField )
		 {
			  CheckClear();
			  HasLabelField = true;
			  this.LabelFieldConflict = labelField;
			  return @delegate.LabelField( labelField );
		 }

		 public override bool StartId( long id )
		 {
			  CheckClear();
			  HasLongStartId = true;
			  LongStartId = id;
			  return @delegate.StartId( id );
		 }

		 public override bool StartId( object id, Group group )
		 {
			  CheckClear();
			  ObjectStartId = id;
			  StartIdGroup = group;
			  return @delegate.StartId( id, group );
		 }

		 public override bool EndId( long id )
		 {
			  CheckClear();
			  HasLongEndId = true;
			  LongEndId = id;
			  return @delegate.EndId( id );
		 }

		 public override bool EndId( object id, Group group )
		 {
			  CheckClear();
			  ObjectEndId = id;
			  EndIdGroup = group;
			  return @delegate.EndId( id, group );
		 }

		 public override bool Type( int type )
		 {
			  CheckClear();
			  HasIntType = true;
			  IntType = type;
			  return @delegate.Type( type );
		 }

		 public override bool Type( string type )
		 {
			  CheckClear();
			  StringType = type;
			  return @delegate.Type( type );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endOfEntity() throws java.io.IOException
		 public override void EndOfEntity()
		 {
			  // Mark that the next call to any data method should clear the state
			  _end = true;
			  @delegate.EndOfEntity();
		 }

		 public virtual string[] Labels()
		 {
			  return LabelsConflict.ToArray();
		 }

		 public virtual object[] Properties()
		 {
			  return PropertiesConflict.ToArray();
		 }

		 public virtual object Id()
		 {
			  return HasLongId ? LongId : ObjectId;
		 }

		 public virtual object EndId()
		 {
			  return HasLongEndId ? LongEndId : ObjectEndId;
		 }

		 public virtual object StartId()
		 {
			  return HasLongStartId ? LongStartId : ObjectStartId;
		 }

		 private void CheckClear()
		 {
			  if ( _end )
			  {
					Clear();
			  }
		 }

		 private void Clear()
		 {
			  _end = false;
			  HasPropertyId = false;
			  PropertyIdConflict = -1;
			  HasIntPropertyKeyIds = false;
			  PropertiesConflict.Clear();
			  HasLongId = false;
			  LongId = -1;
			  ObjectId = null;
			  IdGroup = Group_Fields.Global;
			  LabelsConflict.Clear();
			  HasLabelField = false;
			  LabelFieldConflict = -1;
			  HasLongStartId = false;
			  LongStartId = -1;
			  ObjectStartId = null;
			  StartIdGroup = Group_Fields.Global;
			  HasLongEndId = false;
			  LongEndId = -1;
			  ObjectEndId = null;
			  EndIdGroup = Group_Fields.Global;
			  HasIntType = false;
			  IntType = -1;
			  StringType = null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.Dispose();
		 }

		 public virtual int PropertyCount()
		 {
			  return PropertiesConflict.Count / 2;
		 }

		 public virtual object PropertyKey( int i )
		 {
			  return PropertiesConflict[i * 2];
		 }

		 public virtual object PropertyValue( int i )
		 {
			  return PropertiesConflict[i * 2 + 1];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void replayOnto(InputEntityVisitor visitor) throws java.io.IOException
		 public virtual void ReplayOnto( InputEntityVisitor visitor )
		 {
			  // properties
			  if ( HasPropertyId )
			  {
					visitor.PropertyId( PropertyIdConflict );
			  }
			  else if ( PropertiesConflict.Count > 0 )
			  {
					int propertyCount = propertyCount();
					for ( int i = 0; i < propertyCount; i++ )
					{
						 if ( HasIntPropertyKeyIds )
						 {
							  visitor.property( ( int? ) PropertyKey( i ), PropertyValue( i ) );
						 }
						 else
						 {
							  visitor.Property( ( string ) PropertyKey( i ), PropertyValue( i ) );
						 }
					}
			  }

			  // id
			  if ( HasLongId )
			  {
					visitor.Id( LongId );
			  }
			  else if ( ObjectId != null )
			  {
					visitor.Id( ObjectId, IdGroup );
			  }

			  // labels
			  if ( HasLabelField )
			  {
					visitor.LabelField( LabelFieldConflict );
			  }
			  else if ( LabelsConflict.Count > 0 )
			  {
					visitor.Labels( LabelsConflict.ToArray() );
			  }

			  // start id
			  if ( HasLongStartId )
			  {
					visitor.StartId( LongStartId );
			  }
			  else if ( ObjectStartId != null )
			  {
					visitor.StartId( ObjectStartId, StartIdGroup );
			  }

			  // end id
			  if ( HasLongEndId )
			  {
					visitor.EndId( LongEndId );
			  }
			  else if ( ObjectEndId != null )
			  {
					visitor.EndId( ObjectEndId, EndIdGroup );
			  }

			  // type
			  if ( HasIntType )
			  {
					visitor.Type( IntType );
			  }
			  else if ( !string.ReferenceEquals( StringType, null ) )
			  {
					visitor.Type( StringType );
			  }

			  // all done
			  visitor.EndOfEntity();
		 }
	}

}