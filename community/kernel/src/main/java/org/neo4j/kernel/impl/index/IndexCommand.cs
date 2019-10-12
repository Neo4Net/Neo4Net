using System;
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
namespace Org.Neo4j.Kernel.impl.index
{

	using Org.Neo4j.Graphdb.index;
	using CommandVisitor = Org.Neo4j.Kernel.Impl.Api.CommandVisitor;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using NeoCommandType = Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.write2bLengthAndString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.write3bLengthAndString;

	/// <summary>
	/// Created from <seealso cref="IndexDefineCommand"/> or read from a logical log.
	/// Contains all the different types of commands that an <seealso cref="Index"/> need
	/// to support.
	/// </summary>
	public abstract class IndexCommand : Command
	{
		 public static readonly sbyte ValueTypeNull = ( sbyte ) 0;
		 public static readonly sbyte ValueTypeShort = ( sbyte ) 1;
		 public static readonly sbyte ValueTypeInt = ( sbyte ) 2;
		 public static readonly sbyte ValueTypeLong = ( sbyte ) 3;
		 public static readonly sbyte ValueTypeFloat = ( sbyte ) 4;
		 public static readonly sbyte ValueTypeDouble = ( sbyte ) 5;
		 public static readonly sbyte ValueTypeString = ( sbyte ) 6;

		 private sbyte _commandType;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int IndexNameIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal sbyte EntityTypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long EntityIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int KeyIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal sbyte ValueTypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal object ValueConflict;

		 protected internal virtual void Init( sbyte commandType, int indexNameId, sbyte entityType, long entityId, int keyId, object value )
		 {
			  this._commandType = commandType;
			  this.IndexNameIdConflict = indexNameId;
			  this.EntityTypeConflict = entityType;
			  this.EntityIdConflict = entityId;
			  this.KeyIdConflict = keyId;
			  this.ValueConflict = value;
			  this.ValueTypeConflict = ValueTypeOf( value );
		 }

		 public virtual int IndexNameId
		 {
			 get
			 {
				  return IndexNameIdConflict;
			 }
		 }

		 public virtual sbyte EntityType
		 {
			 get
			 {
				  return EntityTypeConflict;
			 }
		 }

		 public virtual long EntityId
		 {
			 get
			 {
				  return EntityIdConflict;
			 }
		 }

		 public virtual int KeyId
		 {
			 get
			 {
				  return KeyIdConflict;
			 }
		 }

		 public virtual object Value
		 {
			 get
			 {
				  return ValueConflict;
			 }
		 }

		 public virtual sbyte StartNodeNeedsLong()
		 {
			  return 0;
		 }

		 public virtual sbyte EndNodeNeedsLong()
		 {
			  return 0;
		 }

		 private static sbyte ValueTypeOf( object value )
		 {
			  sbyte valueType = 0;
			  if ( value == null )
			  {
					valueType = ValueTypeNull;
			  }
			  else if ( value is Number )
			  {
					if ( value is float? )
					{
						 valueType = ValueTypeFloat;
					}
					else if ( value is double? )
					{
						 valueType = ValueTypeDouble;
					}
					else if ( value is long? )
					{
						 valueType = ValueTypeLong;
					}
					else if ( value is short? )
					{
						 valueType = ValueTypeShort;
					}
					else
					{
						 valueType = ValueTypeInt;
					}
			  }
			  else
			  {
					valueType = ValueTypeString;
			  }
			  return valueType;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeToFile(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 protected internal virtual void WriteToFile( WritableChannel channel )
		 {
			  /* c: commandType
			   * e: entityType
			   * n: indexNameId
			   * k: keyId
			   * i: entityId
			   * v: value type
			   * u: value
			   * x: 0=entityId needs 4b, 1=entityId needs 8b
			   * y: 0=startNode needs 4b, 1=startNode needs 8b
			   * z: 0=endNode needs 4b, 1=endNode needs 8b
			   *
			   * [cccv,vvex][yznn,nnnn][kkkk,kkkk]
			   * [iiii,iiii] x 4 or 8
			   * (either string value)
			   * [llll,llll][llll,llll][llll,llll][string chars...]
			   * (numeric value)
			   * [uuuu,uuuu] x 2-8 (depending on value type)
			   */
			  WriteIndexCommandHeader( channel );
			  PutIntOrLong( channel, EntityId );
			  // Value
			  object value = Value;
			  switch ( ValueType )
			  {
			  case IndexCommand.ValueTypeString:
					write3bLengthAndString( channel, value.ToString() );
					break;
			  case IndexCommand.ValueTypeShort:
					channel.PutShort( ( ( Number ) value ).shortValue() );
					break;
			  case IndexCommand.ValueTypeInt:
					channel.PutInt( ( ( Number ) value ).intValue() );
					break;
			  case IndexCommand.ValueTypeLong:
					channel.PutLong( ( ( Number ) value ).longValue() );
					break;
			  case IndexCommand.ValueTypeFloat:
					channel.PutFloat( ( ( Number ) value ).floatValue() );
					break;
			  case IndexCommand.ValueTypeDouble:
					channel.PutDouble( ( ( Number ) value ).doubleValue() );
					break;
			  case IndexCommand.ValueTypeNull:
					break;
			  default:
					throw new Exception( "Unknown value type " + ValueType );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeIndexCommandHeader(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 protected internal virtual void WriteIndexCommandHeader( WritableChannel channel )
		 {
			  WriteIndexCommandHeader( channel, ValueType, EntityType, NeedsLong( EntityId ), StartNodeNeedsLong(), EndNodeNeedsLong(), IndexNameId, KeyId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static void writeIndexCommandHeader(org.neo4j.storageengine.api.WritableChannel channel, byte valueType, byte entityType, byte entityIdNeedsLong, byte startNodeNeedsLong, byte endNodeNeedsLong, int indexNameId, int keyId) throws java.io.IOException
		 protected internal static void WriteIndexCommandHeader( WritableChannel channel, sbyte valueType, sbyte entityType, sbyte entityIdNeedsLong, sbyte startNodeNeedsLong, sbyte endNodeNeedsLong, int indexNameId, int keyId )
		 {
			  channel.Put( ( sbyte )( ( valueType << 2 ) | ( entityType << 1 ) | entityIdNeedsLong ) );
			  channel.Put( ( sbyte )( ( startNodeNeedsLong << 7 ) | ( endNodeNeedsLong << 6 ) ) );
			  channel.PutShort( ( short ) indexNameId );
			  channel.PutShort( ( short ) keyId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void putIntOrLong(org.neo4j.storageengine.api.WritableChannel channel, long id) throws java.io.IOException
		 protected internal virtual void PutIntOrLong( WritableChannel channel, long id )
		 {
			  if ( NeedsLong( id ) == 1 )
			  {
					channel.PutLong( id );
			  }
			  else
			  {
					channel.PutInt( ( int ) id );
			  }
		 }

		 public class AddNodeCommand : IndexCommand
		 {
			  public virtual void Init( int indexNameId, long entityId, int keyId, object value )
			  {
					base.Init( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexAddCommand, indexNameId, IndexEntityType.Node.id(), entityId, keyId, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
			  public override bool Handle( CommandVisitor visitor )
			  {
					return visitor.VisitIndexAddNodeCommand( this );
			  }

			  public override string ToString()
			  {
					return "AddNode[index:" + IndexNameIdConflict + ", id:" + EntityIdConflict + ", key:" + KeyIdConflict + ", value:" + ValueConflict + "]";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexAddCommand );
					WriteToFile( channel );
			  }
		 }

		 protected internal static sbyte NeedsLong( long value )
		 {
			  return value > int.MaxValue ? ( sbyte )1 : ( sbyte )0;
		 }

		 public class AddRelationshipCommand : IndexCommand
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long StartNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long EndNodeConflict;

			  public virtual void Init( int indexNameId, long entityId, int keyId, object value, long startNode, long endNode )
			  {
					base.Init( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexAddRelationshipCommand, indexNameId, IndexEntityType.Relationship.id(), entityId, keyId, value );
					this.StartNodeConflict = startNode;
					this.EndNodeConflict = endNode;
			  }

			  public virtual long StartNode
			  {
				  get
				  {
						return StartNodeConflict;
				  }
			  }

			  public virtual long EndNode
			  {
				  get
				  {
						return EndNodeConflict;
				  }
			  }

			  public override sbyte StartNodeNeedsLong()
			  {
					return NeedsLong( StartNodeConflict );
			  }

			  public override sbyte EndNodeNeedsLong()
			  {
					return NeedsLong( EndNodeConflict );
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					if ( !base.Equals( o ) )
					{
						 return false;
					}
					AddRelationshipCommand that = ( AddRelationshipCommand ) o;
					return StartNodeConflict == that.StartNodeConflict && EndNodeConflict == that.EndNodeConflict;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( base.GetHashCode(), StartNodeConflict, EndNodeConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
			  public override bool Handle( CommandVisitor visitor )
			  {
					return visitor.VisitIndexAddRelationshipCommand( this );
			  }

			  public override string ToString()
			  {
					return "AddRelationship[index:" + IndexNameIdConflict + ", id:" + EntityIdConflict + ", key:" + KeyIdConflict +
							  ", value:" + ValueConflict + "(" + ( ValueConflict != null ? ValueConflict.GetType().Name : "null" ) + ")" +
							  ", startNode:" + StartNodeConflict +
							  ", endNode:" + EndNodeConflict +
							  "]";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexAddRelationshipCommand );
					WriteToFile( channel );
					PutIntOrLong( channel, StartNode );
					PutIntOrLong( channel, EndNode );
			  }
		 }

		 public class RemoveCommand : IndexCommand
		 {
			  public virtual void Init( int indexNameId, sbyte entityType, long entityId, int keyId, object value )
			  {
					base.Init( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexRemoveCommand, indexNameId, entityType, entityId, keyId, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
			  public override bool Handle( CommandVisitor visitor )
			  {
					return visitor.VisitIndexRemoveCommand( this );
			  }

			  public override string ToString()
			  {
					return format( "Remove%s[index:%d, id:%d, key:%d, value:%s]", IndexEntityType.byId( EntityTypeConflict ).nameToLowerCase(), IndexNameIdConflict, EntityIdConflict, KeyIdConflict, ValueConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexRemoveCommand );
					WriteToFile( channel );
			  }
		 }

		 public class DeleteCommand : IndexCommand
		 {
			  public virtual void Init( int indexNameId, sbyte entityType )
			  {
					base.Init( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexDeleteCommand, indexNameId, entityType, 0L, ( sbyte )0, null );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
			  public override bool Handle( CommandVisitor visitor )
			  {
					return visitor.VisitIndexDeleteCommand( this );
			  }

			  public override string ToString()
			  {
					return "Delete[index:" + IndexNameIdConflict + ", type:" + IndexEntityType.byId( EntityTypeConflict ).nameToLowerCase() + "]";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexDeleteCommand );
					WriteIndexCommandHeader( channel );
			  }
		 }

		 public class CreateCommand : IndexCommand
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IDictionary<string, string> ConfigConflict;

			  public virtual void Init( int indexNameId, sbyte entityType, IDictionary<string, string> config )
			  {
					base.Init( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexCreateCommand, indexNameId, entityType, 0L, ( sbyte )0, null );
					this.ConfigConflict = config;
			  }

			  public virtual IDictionary<string, string> Config
			  {
				  get
				  {
						return ConfigConflict;
				  }
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					if ( !base.Equals( o ) )
					{
						 return false;
					}
					CreateCommand that = ( CreateCommand ) o;
					return Objects.Equals( ConfigConflict, that.ConfigConflict );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( base.GetHashCode(), ConfigConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
			  public override bool Handle( CommandVisitor visitor )
			  {
					return visitor.VisitIndexCreateCommand( this );
			  }

			  public override string ToString()
			  {
					return format( "Create%sIndex[index:%d, config:%s]", IndexEntityType.byId( EntityTypeConflict ).nameToLowerCase(), IndexNameIdConflict, ConfigConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexCreateCommand );
					WriteIndexCommandHeader( channel );
					channel.PutShort( ( short ) Config.Count );
					foreach ( KeyValuePair<string, string> entry in Config.SetOfKeyValuePairs() )
					{
						 write2bLengthAndString( channel, entry.Key );
						 write2bLengthAndString( channel, entry.Value );
					}
			  }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  if ( !base.Equals( o ) )
			  {
					return false;
			  }
			  IndexCommand that = ( IndexCommand ) o;
			  return _commandType == that._commandType && IndexNameIdConflict == that.IndexNameIdConflict && EntityTypeConflict == that.EntityTypeConflict && EntityIdConflict == that.EntityIdConflict && KeyIdConflict == that.KeyIdConflict && ValueTypeConflict == that.ValueTypeConflict && Objects.Equals( ValueConflict, that.ValueConflict );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( base.GetHashCode(), _commandType, IndexNameIdConflict, EntityTypeConflict, EntityIdConflict, KeyIdConflict, ValueTypeConflict, ValueConflict );
		 }

		 public virtual sbyte CommandType
		 {
			 get
			 {
				  return _commandType;
			 }
		 }

		 public virtual sbyte ValueType
		 {
			 get
			 {
				  return ValueTypeConflict;
			 }
		 }
	}

}