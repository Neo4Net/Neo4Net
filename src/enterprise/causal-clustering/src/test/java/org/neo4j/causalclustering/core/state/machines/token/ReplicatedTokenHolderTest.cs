using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.state.machines.token
{
	using Test = org.junit.Test;


	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReplicatedTokenHolderTest
	{
		 private System.Func _storageEngineSupplier = mock( typeof( System.Func ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreInitialTokens()
		 public virtual void ShouldStoreInitialTokens()
		 {
			  // given
			  TokenRegistry registry = new TokenRegistry( "Label" );
			  ReplicatedTokenHolder tokenHolder = new ReplicatedLabelTokenHolder( registry, null, null, _storageEngineSupplier );

			  // when
			  tokenHolder.InitialTokens = asList( new NamedToken( "name1", 1 ), new NamedToken( "name2", 2 ) );

			  // then
			  assertThat( tokenHolder.AllTokens, hasItems( new NamedToken( "name1", 1 ), new NamedToken( "name2", 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnExistingTokenId()
		 public virtual void ShouldReturnExistingTokenId()
		 {
			  // given
			  TokenRegistry registry = new TokenRegistry( "Label" );
			  ReplicatedTokenHolder tokenHolder = new ReplicatedLabelTokenHolder( registry, null, null, _storageEngineSupplier );
			  tokenHolder.InitialTokens = asList( new NamedToken( "name1", 1 ), new NamedToken( "name2", 2 ) );

			  // when
			  int? tokenId = tokenHolder.GetOrCreateId( "name1" );

			  // then
			  assertThat( tokenId, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTokenRequestForNewToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTokenRequestForNewToken()
		 {
			  // given
			  StorageEngine storageEngine = MockedStorageEngine();
			  when( _storageEngineSupplier.get() ).thenReturn(storageEngine);

			  IdGeneratorFactory idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
			  IdGenerator idGenerator = mock( typeof( IdGenerator ) );
			  when( idGenerator.NextId() ).thenReturn(1L);

			  when( idGeneratorFactory.Get( any( typeof( IdType ) ) ) ).thenReturn( idGenerator );

			  TokenRegistry registry = new TokenRegistry( "Label" );
			  int generatedTokenId = 1;
			  ReplicatedTokenHolder tokenHolder = new ReplicatedLabelTokenHolder(registry, (content, trackResult) =>
			  {
				CompletableFuture<object> completeFuture = new CompletableFuture<object>();
				completeFuture.complete( generatedTokenId );
				return completeFuture;
			  }, idGeneratorFactory, _storageEngineSupplier);

			  // when
			  int? tokenId = tokenHolder.GetOrCreateId( "name1" );

			  // then
			  assertThat( tokenId, equalTo( generatedTokenId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.Neo4Net.storageengine.api.StorageEngine mockedStorageEngine() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private StorageEngine MockedStorageEngine()
		 {
			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  doAnswer(invocation =>
			  {
				ICollection<StorageCommand> target = invocation.getArgument( 0 );
				ReadableTransactionState txState = invocation.getArgument( 1 );
				txState.accept( new AdapterAnonymousInnerClass( this, target ) );
				return null;
			  }).when( storageEngine ).createCommands( anyCollection(), any(typeof(ReadableTransactionState)), any(typeof(StorageReader)), any(typeof(ResourceLocker)), anyLong(), any(typeof(TxStateVisitor.Decorator)) );

			  StorageReader readLayer = mock( typeof( StorageReader ) );
			  when( storageEngine.NewReader() ).thenReturn(readLayer);
			  return storageEngine;
		 }

		 private class AdapterAnonymousInnerClass : Neo4Net.Storageengine.Api.txstate.TxStateVisitor_Adapter
		 {
			 private readonly ReplicatedTokenHolderTest _outerInstance;

			 private ICollection<StorageCommand> _target;

			 public AdapterAnonymousInnerClass( ReplicatedTokenHolderTest outerInstance, ICollection<StorageCommand> target )
			 {
				 this.outerInstance = outerInstance;
				 this._target = target;
			 }

			 public override void visitCreatedLabelToken( long id, string name )
			 {
				  LabelTokenRecord before = new LabelTokenRecord( id );
				  LabelTokenRecord after = before.Clone();
				  after.InUse = true;
				  _target.add( new Command.LabelTokenCommand( before, after ) );
			 }
		 }
	}

}