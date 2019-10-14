using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using CommandVisitor = Neo4Net.Kernel.Impl.Api.CommandVisitor;
	using PropertyRecordChange = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyRecordChange;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bitFlag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bitFlags;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IdPrettyPrinter.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IdPrettyPrinter.relationshipType;

	/// <summary>
	/// Command implementations for all the commands that can be performed on a Neo
	/// store.
	/// </summary>
	public abstract class Command : StorageCommand
	{
		public abstract void Serialize( WritableChannel channel );
		 private int _keyHash;
		 private long _key;
		 private Mode _mode;

		 /*
		  * TODO: This is techdebt
		  * This is used to control the order of how commands are applied, which is done because
		  * we don't take read locks, and so the order or how we change things lowers the risk
		  * of reading invalid state. This should be removed once eg. MVCC or read locks has been
		  * implemented.
		  */
		 public sealed class Mode
		 {
			  public static readonly Mode Create = new Mode( "Create", InnerEnum.Create );
			  public static readonly Mode Update = new Mode( "Update", InnerEnum.Update );
			  public static readonly Mode Delete = new Mode( "Delete", InnerEnum.Delete );

			  private static readonly IList<Mode> valueList = new List<Mode>();

			  static Mode()
			  {
				  valueList.Add( Create );
				  valueList.Add( Update );
				  valueList.Add( Delete );
			  }

			  public enum InnerEnum
			  {
				  Create,
				  Update,
				  Delete
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Mode( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public static Mode FromRecordState( bool created, bool inUse )
			  {
					if ( !inUse )
					{
						 return DELETE;
					}
					if ( created )
					{
						 return CREATE;
					}
					return UPDATE;
			  }

			  public static Mode FromRecordState( Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord record )
			  {
					return FromRecordState( record.Created, record.InUse() );
			  }

			 public static IList<Mode> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Mode valueOf( string name )
			 {
				 foreach ( Mode enumInstance in Mode.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 protected internal void Setup( long key, Mode mode )
		 {
			  this._mode = mode;
			  this._keyHash = ( int )( ( ( long )( ( ulong )key >> 32 ) ) ^ key );
			  this._key = key;
		 }

		 public override int GetHashCode()
		 {
			  return _keyHash;
		 }

		 // Force implementors to implement toString
		 public override abstract String ();

		 public virtual long Key
		 {
			 get
			 {
				  return _key;
			 }
		 }

		 public virtual Mode GetMode()
		 {
			  return _mode;
		 }

		 public override bool Equals( object o )
		 {
			  return o != null && o.GetType().Equals(this.GetType()) && Key == ((Command) o).Key;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException;
		 public abstract bool Handle( CommandVisitor handler );

		 protected internal virtual string BeforeAndAfterToString( AbstractBaseRecord before, AbstractBaseRecord after )
		 {
			  return format( " -%s%n         +%s", before, after );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDynamicRecords(org.neo4j.storageengine.api.WritableChannel channel, java.util.Collection<org.neo4j.kernel.impl.store.record.DynamicRecord> records) throws java.io.IOException
		 internal virtual void WriteDynamicRecords( WritableChannel channel, ICollection<DynamicRecord> records )
		 {
			  WriteDynamicRecords( channel, records, records.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDynamicRecords(org.neo4j.storageengine.api.WritableChannel channel, Iterable<org.neo4j.kernel.impl.store.record.DynamicRecord> records, int size) throws java.io.IOException
		 internal virtual void WriteDynamicRecords( WritableChannel channel, IEnumerable<DynamicRecord> records, int size )
		 {
			  channel.PutInt( size ); // 4
			  foreach ( DynamicRecord record in records )
			  {
					WriteDynamicRecord( channel, record );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeDynamicRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.DynamicRecord record) throws java.io.IOException
		 internal virtual void WriteDynamicRecord( WritableChannel channel, DynamicRecord record )
		 {
			  // id+type+in_use(byte)+nr_of_bytes(int)+next_block(long)
			  if ( record.InUse() )
			  {
					sbyte inUse = Record.IN_USE.byteValue();
					if ( record.StartRecord )
					{
						 inUse |= Record.FIRST_IN_CHAIN.byteValue();
					}
					channel.PutLong( record.Id ).putInt( record.TypeAsInt ).put( inUse ).putInt( record.Length ).putLong( record.NextBlock );
					sbyte[] data = record.Data;
					Debug.Assert( data != null );
					channel.Put( data, data.Length );
			  }
			  else
			  {
					sbyte inUse = Record.NOT_IN_USE.byteValue();
					channel.PutLong( record.Id ).putInt( record.TypeAsInt ).put( inUse );
			  }
		 }

		 public abstract class BaseCommand<RECORD> : Command where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly RECORD BeforeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly RECORD AfterConflict;

			  public BaseCommand( RECORD before, RECORD after )
			  {
					outerInstance.setup( after.Id, Mode.fromRecordState( after ) );
					this.BeforeConflict = before;
					this.AfterConflict = after;
			  }

			  public override string ToString()
			  {
					return outerInstance.beforeAndAfterToString( BeforeConflict, AfterConflict );
			  }

			  public virtual RECORD Before
			  {
				  get
				  {
						return BeforeConflict;
				  }
			  }

			  public virtual RECORD After
			  {
				  get
				  {
						return AfterConflict;
				  }
			  }
		 }

		 public class NodeCommand : BaseCommand<NodeRecord>
		 {
			  public NodeCommand( NodeRecord before, NodeRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitNodeCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.NodeCommand );
					channel.PutLong( AfterConflict.Id );
					WriteNodeRecord( channel, BeforeConflict );
					WriteNodeRecord( channel, AfterConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNodeRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.NodeRecord record) throws java.io.IOException
			  internal virtual void WriteNodeRecord( WritableChannel channel, NodeRecord record )
			  {
					sbyte flags = bitFlags( bitFlag( record.InUse(), Record.IN_USE.byteValue() ), bitFlag(record.Created, Record.CREATED_IN_TX), bitFlag(record.RequiresSecondaryUnit(), Record.REQUIRE_SECONDARY_UNIT), bitFlag(record.HasSecondaryUnitId(), Record.HAS_SECONDARY_UNIT), bitFlag(record.UseFixedReferences, Record.USES_FIXED_REFERENCE_FORMAT) );
					channel.Put( flags );
					if ( record.InUse() )
					{
						 channel.Put( record.Dense ? ( sbyte ) 1 : ( sbyte ) 0 );
						 channel.PutLong( record.NextRel ).putLong( record.NextProp );
						 channel.PutLong( record.LabelField );
						 if ( record.HasSecondaryUnitId() )
						 {
							  channel.PutLong( record.SecondaryUnitId );
						 }
					}
					// Always write dynamic label records because we want to know which ones have been deleted
					// especially if the node has been deleted.
					outerInstance.writeDynamicRecords( channel, record.DynamicLabelRecords );
			  }
		 }

		 public class RelationshipCommand : BaseCommand<RelationshipRecord>
		 {
			  public RelationshipCommand( RelationshipRecord before, RelationshipRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitRelationshipCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.RelCommand );
					channel.PutLong( AfterConflict.Id );
					WriteRelationshipRecord( channel, BeforeConflict );
					WriteRelationshipRecord( channel, AfterConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRelationshipRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.RelationshipRecord record) throws java.io.IOException
			  internal virtual void WriteRelationshipRecord( WritableChannel channel, RelationshipRecord record )
			  {
					sbyte flags = bitFlags( bitFlag( record.InUse(), Record.IN_USE.byteValue() ), bitFlag(record.Created, Record.CREATED_IN_TX), bitFlag(record.RequiresSecondaryUnit(), Record.REQUIRE_SECONDARY_UNIT), bitFlag(record.HasSecondaryUnitId(), Record.HAS_SECONDARY_UNIT), bitFlag(record.UseFixedReferences, Record.USES_FIXED_REFERENCE_FORMAT) );
					channel.Put( flags );
					if ( record.InUse() )
					{
						 channel.PutLong( record.FirstNode ).putLong( record.SecondNode ).putInt( record.Type ).putLong( record.FirstPrevRel ).putLong( record.FirstNextRel ).putLong( record.SecondPrevRel ).putLong( record.SecondNextRel ).putLong( record.NextProp ).put( ( sbyte )( ( record.FirstInFirstChain ? 1 : 0 ) | ( record.FirstInSecondChain ? 2 : 0 ) ) );
						 if ( record.HasSecondaryUnitId() )
						 {
							  channel.PutLong( record.SecondaryUnitId );
						 }
					}
					else
					{
						 channel.PutInt( record.Type );
					}
			  }
		 }

		 public class RelationshipGroupCommand : BaseCommand<RelationshipGroupRecord>
		 {
			  public RelationshipGroupCommand( RelationshipGroupRecord before, RelationshipGroupRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitRelationshipGroupCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.RelGroupCommand );
					channel.PutLong( AfterConflict.Id );
					WriteRelationshipGroupRecord( channel, BeforeConflict );
					WriteRelationshipGroupRecord( channel, AfterConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRelationshipGroupRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.RelationshipGroupRecord record) throws java.io.IOException
			  internal virtual void WriteRelationshipGroupRecord( WritableChannel channel, RelationshipGroupRecord record )
			  {
					sbyte flags = bitFlags( bitFlag( record.InUse(), Record.IN_USE.byteValue() ), bitFlag(record.RequiresSecondaryUnit(), Record.REQUIRE_SECONDARY_UNIT), bitFlag(record.HasSecondaryUnitId(), Record.HAS_SECONDARY_UNIT), bitFlag(record.UseFixedReferences, Record.USES_FIXED_REFERENCE_FORMAT) );
					channel.Put( flags );
					channel.PutShort( ( short ) record.Type );
					channel.PutLong( record.Next );
					channel.PutLong( record.FirstOut );
					channel.PutLong( record.FirstIn );
					channel.PutLong( record.FirstLoop );
					channel.PutLong( record.OwningNode );
					if ( record.HasSecondaryUnitId() )
					{
						 channel.PutLong( record.SecondaryUnitId );
					}
			  }
		 }

		 public class NeoStoreCommand : BaseCommand<NeoStoreRecord>
		 {
			  public NeoStoreCommand( NeoStoreRecord before, NeoStoreRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitNeoStoreCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.NeostoreCommand );
					WriteNeoStoreRecord( channel, BeforeConflict );
					WriteNeoStoreRecord( channel, AfterConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNeoStoreRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.NeoStoreRecord record) throws java.io.IOException
			  internal virtual void WriteNeoStoreRecord( WritableChannel channel, NeoStoreRecord record )
			  {
					channel.PutLong( record.NextProp );
			  }
		 }

		 public class PropertyCommand : BaseCommand<PropertyRecord>, PropertyRecordChange
		 {
			  public PropertyCommand( PropertyRecord before, PropertyRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitPropertyCommand( this );
			  }

			  public virtual long EntityId
			  {
				  get
				  {
						if ( AfterConflict.NodeSet )
						{
							 return AfterConflict.NodeId;
						}
						if ( AfterConflict.RelSet )
						{
							 return AfterConflict.RelId;
						}
						throw new System.NotSupportedException( format( "Unexpected owner of property %s, neither a node nor a relationship", AfterConflict ) );
				  }
			  }

			  public virtual long NodeId
			  {
				  get
				  {
						return AfterConflict.NodeId;
				  }
			  }

			  public virtual long RelId
			  {
				  get
				  {
						return AfterConflict.RelId;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.PropCommand );
					channel.PutLong( AfterConflict.Id );
					WritePropertyRecord( channel, BeforeConflict );
					WritePropertyRecord( channel, AfterConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePropertyRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.PropertyRecord record) throws java.io.IOException
			  internal virtual void WritePropertyRecord( WritableChannel channel, PropertyRecord record )
			  {
					sbyte flags = bitFlags( bitFlag( record.InUse(), Record.IN_USE.byteValue() ), bitFlag(record.RelId != -1, Record.REL_PROPERTY.byteValue()), bitFlag(record.RequiresSecondaryUnit(), Record.REQUIRE_SECONDARY_UNIT), bitFlag(record.HasSecondaryUnitId(), Record.HAS_SECONDARY_UNIT), bitFlag(record.UseFixedReferences, Record.USES_FIXED_REFERENCE_FORMAT) );

					channel.Put( flags ); // 1
					channel.PutLong( record.NextProp ).putLong( record.PrevProp ); // 8 + 8
					long nodeId = record.NodeId;
					long relId = record.RelId;
					if ( nodeId != -1 )
					{
						 channel.PutLong( nodeId ); // 8 or
					}
					else if ( relId != -1 )
					{
						 channel.PutLong( relId ); // 8 or
					}
					else
					{
						 // means this records value has not changed, only place in
						 // prop chain
						 channel.PutLong( -1 ); // 8
					}
					if ( record.HasSecondaryUnitId() )
					{
						 channel.PutLong( record.SecondaryUnitId );
					}
					channel.Put( ( sbyte ) record.NumberOfProperties() ); // 1
					foreach ( PropertyBlock block in record )
					{
						 Debug.Assert( block.Size > 0, record + " seems kinda broken" );
						 WritePropertyBlock( channel, block );
					}
					outerInstance.writeDynamicRecords( channel, record.DeletedRecords );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePropertyBlock(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.PropertyBlock block) throws java.io.IOException
			  internal virtual void WritePropertyBlock( WritableChannel channel, PropertyBlock block )
			  {
					sbyte blockSize = ( sbyte ) block.Size;
					Debug.Assert( blockSize > 0, blockSize + " is not a valid block size value" );
					channel.Put( blockSize ); // 1
					long[] propBlockValues = block.ValueBlocks;
					foreach ( long propBlockValue in propBlockValues )
					{
						 channel.PutLong( propBlockValue );
					}
					/*
					 * For each block we need to keep its dynamic record chain if
					 * it is just created. Deleted dynamic records are in the property
					 * record and dynamic records are never modified. Also, they are
					 * assigned as a whole, so just checking the first should be enough.
					 */
					if ( block.Light )
					{
						 /*
						  *  This has to be int. If this record is not light
						  *  then we have the number of DynamicRecords that follow,
						  *  which is an int. We do not currently want/have a flag bit so
						  *  we simplify by putting an int here always
						  */
						 channel.PutInt( 0 ); // 4 or
					}
					else
					{
						 outerInstance.writeDynamicRecords( channel, block.ValueRecords );
					}
			  }
		 }

		 public abstract class TokenCommand<RECORD> : BaseCommand<RECORD> where RECORD : Neo4Net.Kernel.Impl.Store.Records.TokenRecord
		 {
			  public TokenCommand( RECORD before, RECORD after ) : base( before, after )
			  {
			  }

			  public override string ToString()
			  {
					return outerInstance.beforeAndAfterToString( before, after );
			  }
		 }

		 public class PropertyKeyTokenCommand : TokenCommand<PropertyKeyTokenRecord>
		 {
			  public PropertyKeyTokenCommand( PropertyKeyTokenRecord before, PropertyKeyTokenRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitPropertyKeyTokenCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.PropIndexCommand );
					channel.PutInt( after.IntId );
					WritePropertyKeyTokenRecord( channel, before );
					WritePropertyKeyTokenRecord( channel, after );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePropertyKeyTokenRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.PropertyKeyTokenRecord record) throws java.io.IOException
			  internal virtual void WritePropertyKeyTokenRecord( WritableChannel channel, PropertyKeyTokenRecord record )
			  {
					// id+in_use(byte)+count(int)+key_blockId(int)+nr_key_records(int)
					sbyte inUse = record.InUse() ? Record.IN_USE.byteValue() : Record.NOT_IN_USE.byteValue();
					channel.Put( inUse );
					channel.PutInt( record.PropertyCount ).putInt( record.NameId );
					if ( record.Light )
					{
						 channel.PutInt( 0 );
					}
					else
					{
						 outerInstance.writeDynamicRecords( channel, record.NameRecords );
					}
			  }
		 }

		 public class RelationshipTypeTokenCommand : TokenCommand<RelationshipTypeTokenRecord>
		 {
			  public RelationshipTypeTokenCommand( RelationshipTypeTokenRecord before, RelationshipTypeTokenRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitRelationshipTypeTokenCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.RelTypeCommand );
					channel.PutInt( after.IntId );
					WriteRelationshipTypeTokenRecord( channel, before );
					WriteRelationshipTypeTokenRecord( channel, after );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRelationshipTypeTokenRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.RelationshipTypeTokenRecord record) throws java.io.IOException
			  internal virtual void WriteRelationshipTypeTokenRecord( WritableChannel channel, RelationshipTypeTokenRecord record )
			  {
					// id+in_use(byte)+count(int)+key_blockId(int)+nr_key_records(int)
					sbyte inUse = record.InUse() ? Record.IN_USE.byteValue() : Record.NOT_IN_USE.byteValue();
					channel.Put( inUse );
					channel.PutInt( record.NameId );
					if ( record.Light )
					{
						 channel.PutInt( 0 );
					}
					else
					{
						 outerInstance.writeDynamicRecords( channel, record.NameRecords );
					}
			  }
		 }

		 public class LabelTokenCommand : TokenCommand<LabelTokenRecord>
		 {
			  public LabelTokenCommand( LabelTokenRecord before, LabelTokenRecord after ) : base( before, after )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitLabelTokenCommand( this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.LabelKeyCommand );
					channel.PutInt( after.IntId );
					WriteLabelTokenRecord( channel, before );
					WriteLabelTokenRecord( channel, after );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeLabelTokenRecord(org.neo4j.storageengine.api.WritableChannel channel, org.neo4j.kernel.impl.store.record.LabelTokenRecord record) throws java.io.IOException
			  internal virtual void WriteLabelTokenRecord( WritableChannel channel, LabelTokenRecord record )
			  {
					// id+in_use(byte)+type_blockId(int)+nr_type_records(int)
					sbyte inUse = record.InUse() ? Record.IN_USE.byteValue() : Record.NOT_IN_USE.byteValue();
					channel.Put( inUse ).putInt( record.NameId );
					outerInstance.writeDynamicRecords( channel, record.NameRecords );
			  }
		 }

		 public class SchemaRuleCommand : Command
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SchemaRecord RecordsBeforeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SchemaRecord RecordsAfterConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SchemaRule SchemaRuleConflict;

			  public SchemaRuleCommand( ICollection<DynamicRecord> recordsBefore, ICollection<DynamicRecord> recordsAfter, SchemaRule schemaRule ) : this( new SchemaRecord( recordsBefore ), new SchemaRecord( recordsAfter ), schemaRule )
			  {
			  }

			  public SchemaRuleCommand( SchemaRecord recordsBefore, SchemaRecord recordsAfter, SchemaRule schemaRule )
			  {
					Setup( Iterables.first( recordsAfter ).Id, Mode.fromRecordState( Iterables.first( recordsAfter ) ) );
					this.RecordsBeforeConflict = recordsBefore;
					this.RecordsAfterConflict = recordsAfter;
					this.SchemaRuleConflict = schemaRule;
			  }

			  public override string ToString()
			  {
					if ( SchemaRuleConflict != null )
					{
						 return Mode + ":" + SchemaRuleConflict.ToString();
					}
					return "SchemaRule" + RecordsAfterConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitSchemaRuleCommand( this );
			  }

			  public virtual SchemaRecord RecordsAfter
			  {
				  get
				  {
						return RecordsAfterConflict;
				  }
			  }

			  public virtual SchemaRule SchemaRule
			  {
				  get
				  {
						return SchemaRuleConflict;
				  }
			  }

			  public virtual SchemaRecord RecordsBefore
			  {
				  get
				  {
						return RecordsBeforeConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.SchemaRuleCommand );
					WriteDynamicRecords( channel, RecordsBeforeConflict, RecordsBeforeConflict.size() );
					WriteDynamicRecords( channel, RecordsAfterConflict, RecordsAfterConflict.size() );
					channel.Put( Iterables.first( RecordsAfterConflict ).Created ? ( sbyte ) 1 : 0 );
			  }
		 }

		 public class NodeCountsCommand : Command
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int LabelIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long DeltaConflict;

			  public NodeCountsCommand( int labelId, long delta )
			  {
					Setup( labelId, Mode.Update );
					Debug.Assert( delta != 0, "Tried to create a NodeCountsCommand for something that didn't change any count" );
					this.LabelIdConflict = labelId;
					this.DeltaConflict = delta;
			  }

			  public override string ToString()
			  {
					return string.Format( "UpdateCounts[({0}) {1} {2:D}]", label( LabelIdConflict ), DeltaConflict < 0 ? "-" : "+", Math.Abs( DeltaConflict ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitNodeCountsCommand( this );
			  }

			  public virtual int LabelId()
			  {
					return LabelIdConflict;
			  }

			  public virtual long Delta()
			  {
					return DeltaConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.UpdateNodeCountsCommand );
					channel.PutInt( LabelId() ).putLong(Delta());
			  }
		 }

		 public class RelationshipCountsCommand : Command
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int StartLabelIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int TypeIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int EndLabelIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long DeltaConflict;

			  public RelationshipCountsCommand( int startLabelId, int typeId, int endLabelId, long delta )
			  {
					Setup( typeId, Mode.Update );
					Debug.Assert( delta != 0, "Tried to create a RelationshipCountsCommand for something that didn't change any count" );
					this.StartLabelIdConflict = startLabelId;
					this.TypeIdConflict = typeId;
					this.EndLabelIdConflict = endLabelId;
					this.DeltaConflict = delta;
			  }

			  public override string ToString()
			  {
					return string.Format( "UpdateCounts[({0})-{1}->({2}) {3} {4:D}]", label( StartLabelIdConflict ), relationshipType( TypeIdConflict ), label( EndLabelIdConflict ), DeltaConflict < 0 ? "-" : "+", Math.Abs( DeltaConflict ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor handler) throws java.io.IOException
			  public override bool Handle( CommandVisitor handler )
			  {
					return handler.VisitRelationshipCountsCommand( this );
			  }

			  public virtual int StartLabelId()
			  {
					return StartLabelIdConflict;
			  }

			  public virtual int TypeId()
			  {
					return TypeIdConflict;
			  }

			  public virtual int EndLabelId()
			  {
					return EndLabelIdConflict;
			  }

			  public virtual long Delta()
			  {
					return DeltaConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Serialize( WritableChannel channel )
			  {
					channel.Put( NeoCommandType_Fields.UpdateRelationshipCountsCommand );
					channel.PutInt( StartLabelId() ).putInt(TypeId()).putInt(EndLabelId()).putLong(Delta());
			  }
		 }
	}

}