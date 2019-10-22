using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{

	using Neo4Net.com;
	using Neo4Net.com;
	using Protocol = Neo4Net.com.Protocol;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using Neo4Net.com;
	using ToNetworkStoreWriter = Neo4Net.com.storecopy.ToNetworkStoreWriter;
	using HandshakeResult = Neo4Net.Kernel.ha.com.master.HandshakeResult;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using IdAllocation = Neo4Net.Kernel.ha.id.IdAllocation;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.INTEGER_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.LONG_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.VOID_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.readBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.readString;

	public class HaRequestType210 : AbstractHaRequestTypes
	{
		 public HaRequestType210( LogEntryReader<ReadableClosablePositionAwareChannel> entryReader, ObjectSerializer<LockResult> lockResultObjectSerializer )
		 {
			  RegisterAllocateIds();
			  RegisterCreateRelationshipType();
			  RegisterAcquireExclusiveLock( lockResultObjectSerializer );
			  RegisterAcquireSharedLock( lockResultObjectSerializer );
			  RegisterCommit( entryReader );
			  RegisterPullUpdates();
			  RegisterEndLockSession();
			  RegisterHandshake();
			  RegisterCopyStore();
			  RegisterNewLockSession();
			  RegisterCreatePropertyKey();
			  RegisterCreateLabel();
		 }

		 private void RegisterAllocateIds()
		 {
			  TargetCaller<Master, IdAllocation> allocateIdTarget = ( master, context, input, target ) =>
			  {
				IdType idType = IdType.values()[input.readByte()];
				return master.allocateIds( context, idType );
			  };
			  ObjectSerializer<IdAllocation> allocateIdSerializer = ( idAllocation, result ) =>
			  {
				IdRange idRange = idAllocation.IdRange;
				result.writeInt( idRange.DefragIds.length );
				foreach ( long id in idRange.DefragIds )
				{
					 result.writeLong( id );
				}
				result.writeLong( idRange.RangeStart );
				result.writeInt( idRange.RangeLength );
				result.writeLong( idAllocation.HighestIdInUse );
				result.writeLong( idAllocation.DefragCount );
			  };
			  Register( HaRequestTypes_Type.AllocateIds, allocateIdTarget, allocateIdSerializer );
		 }

		 private void RegisterCreateRelationshipType()
		 {
			  TargetCaller<Master, int> createRelationshipTypeTarget = ( master, context, input, target ) => master.createRelationshipType( context, readString( input ) );
			  Register( HaRequestTypes_Type.CreateRelationshipType, createRelationshipTypeTarget, INTEGER_SERIALIZER );
		 }

		 private void RegisterAcquireExclusiveLock( ObjectSerializer<LockResult> lockResultObjectSerializer )
		 {
			  register(HaRequestTypes_Type.AcquireExclusiveLock, new AcquireLockCallAnonymousInnerClass(this)
			 , lockResultObjectSerializer, true);
		 }

		 private class AcquireLockCallAnonymousInnerClass : AcquireLockCall
		 {
			 private readonly HaRequestType210 _outerInstance;

			 public AcquireLockCallAnonymousInnerClass( HaRequestType210 outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Response<LockResult> @lock( Master master, RequestContext context, ResourceType type, params long[] ids )
			 {
				  return master.AcquireExclusiveLock( context, type, ids );
			 }
		 }

		 private void RegisterAcquireSharedLock( ObjectSerializer<LockResult> lockResultObjectSerializer )
		 {
			  register(HaRequestTypes_Type.AcquireSharedLock, new AcquireLockCallAnonymousInnerClass2(this)
			 , lockResultObjectSerializer, true);
		 }

		 private class AcquireLockCallAnonymousInnerClass2 : AcquireLockCall
		 {
			 private readonly HaRequestType210 _outerInstance;

			 public AcquireLockCallAnonymousInnerClass2( HaRequestType210 outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Response<LockResult> @lock( Master master, RequestContext context, ResourceType type, params long[] ids )
			 {
				  return master.AcquireSharedLock( context, type, ids );
			 }
		 }

		 private void RegisterCommit( LogEntryReader<ReadableClosablePositionAwareChannel> entryReader )
		 {
			  TargetCaller<Master, long> commitTarget = ( master, context, input, target ) =>
			  {
				readString( input ); // Always neostorexadatasource

				TransactionRepresentation tx;
				try
				{
					 Deserializer<TransactionRepresentation> deserializer = new Protocol.TransactionRepresentationDeserializer( entryReader );
					 tx = deserializer.read( input, null );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}

				return master.commit( context, tx );
			  };
			  Register( HaRequestTypes_Type.Commit, commitTarget, LONG_SERIALIZER );
		 }

		 private void RegisterPullUpdates()
		 {
			  TargetCaller<Master, Void> pullUpdatesTarget = ( master, context, input, target ) => master.pullUpdates( context );
			  Register( HaRequestTypes_Type.PullUpdates, pullUpdatesTarget, VOID_SERIALIZER );
		 }

		 private void RegisterEndLockSession()
		 {
			  // NOTE <1>: A 'false' argument for 'unpack' means we won't unpack the response.
			  // We do this because END_LOCK_SESSION request can be send in 3 cases:
			  //  1) transaction committed successfully
			  //  2) transaction rolled back successfully
			  //  3) transaction was terminated
			  // Master's response for this call is an obligation to pull up to a specified txId.
			  // Processing/unpacking of this response is not needed in all 3 cases:
			  //  1) committed transaction pulls transaction stream as part of COMMIT call
			  //  2) rolled back transaction does not care about reading any more
			  //  3) terminated transaction does not care about reading any more
			  TargetCaller<Master, Void> endLockSessionTarget = ( master, context, input, target ) => master.endLockSession( context, readBoolean( input ) );
			  Register( HaRequestTypes_Type.EndLockSession, endLockSessionTarget, VOID_SERIALIZER, false );
		 }

		 private void RegisterHandshake()
		 {
			  TargetCaller<Master, HandshakeResult> handshakeTarget = ( master, context, input, target ) => master.handshake( input.readLong(), null );
			  ObjectSerializer<HandshakeResult> handshakeResultObjectSerializer = ( responseObject, result ) =>
			  {
				result.writeLong( responseObject.txChecksum() );
				result.writeLong( responseObject.epoch() );
			  };
			  Register( HaRequestTypes_Type.Handshake, handshakeTarget, handshakeResultObjectSerializer );
		 }

		 private void RegisterCopyStore()
		 {
			  TargetCaller<Master, Void> copyStoreTarget = ( master, context, input, target ) => master.copyStore( context, new ToNetworkStoreWriter( target, new Monitors() ) );
			  Register( HaRequestTypes_Type.CopyStore, copyStoreTarget, VOID_SERIALIZER, false );
		 }

		 private void RegisterNewLockSession()
		 {
			  TargetCaller<Master, Void> newLockSessionTarget = ( master, context, input, target ) => master.newLockSession( context );
			  Register( HaRequestTypes_Type.NewLockSession, newLockSessionTarget, VOID_SERIALIZER );
		 }

		 private void RegisterCreatePropertyKey()
		 {
			  TargetCaller<Master, int> createPropertyKeyTarget = ( master, context, input, target ) => master.createPropertyKey( context, readString( input ) );
			  Register( HaRequestTypes_Type.CreatePropertyKey, createPropertyKeyTarget, INTEGER_SERIALIZER );
		 }

		 private void RegisterCreateLabel()
		 {
			  TargetCaller<Master, int> createLabelTarget = ( master, context, input, target ) => master.createLabel( context, readString( input ) );
			  Register( HaRequestTypes_Type.CreateLabel, createLabelTarget, INTEGER_SERIALIZER );
		 }
	}

}