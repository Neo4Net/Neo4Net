using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.index
{
	using IntObjectMap = org.eclipse.collections.api.map.primitive.IntObjectMap;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableObjectIntMap = org.eclipse.collections.api.map.primitive.MutableObjectIntMap;
	using ObjectIntMap = org.eclipse.collections.api.map.primitive.ObjectIntMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;


	using CommandVisitor = Neo4Net.Kernel.Impl.Api.CommandVisitor;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NeoCommandType = Neo4Net.Kernel.impl.transaction.command.NeoCommandType;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;
	using VisibleForTesting = Neo4Net.Util.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.write2bLengthAndString;

	/// <summary>
	/// A command which have to be first in the transaction. It will map index names
	/// and keys to ids so that all other commands in that transaction only refer
	/// to ids instead of names. This reduced the number of bytes needed for commands
	/// roughly 50% for transaction with more than a couple of commands in it,
	/// depending on the size of the value.
	/// 
	/// After this command has been created it will act as a factory for other
	/// commands so that it can spit out correct index name and key ids.
	/// </summary>
	public class IndexDefineCommand : Command
	{
		 internal static readonly int HighestPossibleId = 0xFFFF - 1; // -1 since the actual value -1 is reserved for all-ones
		 private readonly AtomicInteger _nextIndexNameId = new AtomicInteger();
		 private readonly AtomicInteger _nextKeyId = new AtomicInteger();
		 private MutableObjectIntMap<string> _indexNameIdRange = new ObjectIntHashMap<string>();
		 private MutableObjectIntMap<string> _keyIdRange = new ObjectIntHashMap<string>();
		 private MutableIntObjectMap<string> _idToIndexName = new IntObjectHashMap<string>();
		 private MutableIntObjectMap<string> _idToKey = new IntObjectHashMap<string>();

		 public virtual void Init( MutableObjectIntMap<string> indexNames, MutableObjectIntMap<string> keys )
		 {
			  this._indexNameIdRange = requireNonNull( indexNames, "indexNames" );
			  this._keyIdRange = requireNonNull( keys, "keys" );
			  this._idToIndexName = indexNames.flipUniqueValues();
			  this._idToKey = keys.flipUniqueValues();
		 }

		 private static string GetFromMap( IntObjectMap<string> map, int id )
		 {
			  if ( id == -1 )
			  {
					return null;
			  }
			  string result = map.get( id );
			  if ( string.ReferenceEquals( result, null ) )
			  {
					throw new System.ArgumentException( id.ToString() );
			  }
			  return result;
		 }

		 public virtual string GetIndexName( int id )
		 {
			  return GetFromMap( _idToIndexName, id );
		 }

		 public virtual string GetKey( int id )
		 {
			  return GetFromMap( _idToKey, id );
		 }

		 public virtual int GetOrAssignIndexNameId( string indexName )
		 {
			  return GetOrAssignId( _indexNameIdRange, _idToIndexName, _nextIndexNameId, indexName );
		 }

		 public virtual int GetOrAssignKeyId( string key )
		 {
			  return GetOrAssignId( _keyIdRange, _idToKey, _nextKeyId, key );
		 }

		 private int GetOrAssignId( MutableObjectIntMap<string> stringToId, MutableIntObjectMap<string> idToString, AtomicInteger nextId, string @string )
		 {
			  if ( string.ReferenceEquals( @string, null ) )
			  {
					return -1;
			  }

			  if ( stringToId.containsKey( @string ) )
			  {
					return stringToId.get( @string );
			  }

			  int id = nextId.incrementAndGet();
			  if ( id > HighestPossibleId || stringToId.size() >= HighestPossibleId )
			  {
					throw new System.InvalidOperationException( format( "Modifying more than %d indexes or keys in a single transaction is not supported", HighestPossibleId + 1 ) );
			  }

			  stringToId.put( @string, id );
			  idToString.put( id, @string );
			  return id;
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
			  IndexDefineCommand that = ( IndexDefineCommand ) o;
			  return _nextIndexNameId.get() == that._nextIndexNameId.get() && _nextKeyId.get() == that._nextKeyId.get() && Objects.Equals(_indexNameIdRange, that._indexNameIdRange) && Objects.Equals(_keyIdRange, that._keyIdRange) && Objects.Equals(_idToIndexName, that._idToIndexName) && Objects.Equals(_idToKey, that._idToKey);
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( base.GetHashCode(), _nextIndexNameId.get(), _nextKeyId.get(), _indexNameIdRange, _keyIdRange, _idToIndexName, _idToKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean handle(org.neo4j.kernel.impl.api.CommandVisitor visitor) throws java.io.IOException
		 public override bool Handle( CommandVisitor visitor )
		 {
			  return visitor.VisitIndexDefineCommand( this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting ObjectIntMap<String> getIndexNameIdRange()
		 internal virtual ObjectIntMap<string> IndexNameIdRange
		 {
			 get
			 {
				  return _indexNameIdRange;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting ObjectIntMap<String> getKeyIdRange()
		 internal virtual ObjectIntMap<string> KeyIdRange
		 {
			 get
			 {
				  return _keyIdRange;
			 }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[names:" + _indexNameIdRange + ", keys:" + _keyIdRange + "]";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public override void Serialize( WritableChannel channel )
		 {
			  channel.Put( Neo4Net.Kernel.impl.transaction.command.NeoCommandType_Fields.IndexDefineCommand );
			  sbyte zero = 0;
			  IndexCommand.WriteIndexCommandHeader( channel, zero, zero, zero, zero, zero, zero, zero );
			  try
			  {
					WriteMap( channel, _indexNameIdRange );
					WriteMap( channel, _keyIdRange );
			  }
			  catch ( UncheckedIOException e )
			  {
					throw new IOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeMap(org.neo4j.storageengine.api.WritableChannel channel, org.eclipse.collections.api.map.primitive.ObjectIntMap<String> map) throws java.io.IOException
		 private static void WriteMap( WritableChannel channel, ObjectIntMap<string> map )
		 {
			  Debug.Assert( map.size() <= HighestPossibleId, "Can not write map with size larger than 2 bytes. Actual size " + map.size() );
			  channel.PutShort( ( short ) map.size() );

			  map.forEachKeyValue((key, id) =>
			  {
				Debug.Assert( id <= HighestPossibleId, "Can not write id larger than 2 bytes. Actual value " + id );
				try
				{
					 write2bLengthAndString( channel, key );
					 channel.PutShort( ( short ) id );
				}
				catch ( IOException e )
				{
					 throw new UncheckedIOException( e );
				}
			  });
		 }
	}

}