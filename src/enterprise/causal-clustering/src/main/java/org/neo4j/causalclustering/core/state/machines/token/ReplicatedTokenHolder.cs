using System;
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

	using ReplicationFailureException = Neo4Net.causalclustering.core.replication.ReplicationFailureException;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.CreateConstraintFailureException;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using AbstractTokenHolderBase = Neo4Net.Kernel.impl.core.AbstractTokenHolderBase;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ResourceLocker = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceLocker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Fields.NO_DECORATION;

	public class ReplicatedTokenHolder : AbstractTokenHolderBase
	{
		 private readonly Replicator _replicator;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly IdType _tokenIdType;
		 private readonly TokenType _type;
		 private readonly System.Func<StorageEngine> _storageEngineSupplier;
		 private readonly ReplicatedTokenCreator _tokenCreator;

		 // TODO: Clean up all the resolving, which now happens every time with special selection strategies.
		 internal ReplicatedTokenHolder( TokenRegistry tokenRegistry, Replicator replicator, IdGeneratorFactory idGeneratorFactory, IdType tokenIdType, System.Func<StorageEngine> storageEngineSupplier, TokenType type, ReplicatedTokenCreator tokenCreator ) : base( tokenRegistry )
		 {
			  this._replicator = replicator;
			  this._idGeneratorFactory = idGeneratorFactory;
			  this._tokenIdType = tokenIdType;
			  this._type = type;
			  this._storageEngineSupplier = storageEngineSupplier;
			  this._tokenCreator = tokenCreator;
		 }

		 public override void GetOrCreateIds( string[] names, int[] ids )
		 {
			  // todo This could be optimised, but doing so requires a protocol change.
			  for ( int i = 0; i < names.Length; i++ )
			  {
					ids[i] = GetOrCreateId( names[i] );
			  }
		 }

		 protected internal override int CreateToken( string tokenName )
		 {
			  ReplicatedTokenRequest tokenRequest = new ReplicatedTokenRequest( _type, tokenName, CreateCommands( tokenName ) );
			  try
			  {
					Future<object> future = _replicator.replicate( tokenRequest, true );
					return ( int ) future.get();
			  }
			  catch ( Exception e ) when ( e is ReplicationFailureException || e is InterruptedException )
			  {
					throw new Neo4Net.GraphDb.TransactionFailureException( "Could not create token", e );
			  }
			  catch ( ExecutionException e )
			  {
					throw new System.InvalidOperationException( e );
			  }
		 }

		 private sbyte[] CreateCommands( string tokenName )
		 {
			  StorageEngine storageEngine = _storageEngineSupplier.get();
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  TransactionState txState = new TxState();
			  int tokenId = Math.toIntExact( _idGeneratorFactory.get( _tokenIdType ).nextId() );
			  _tokenCreator.createToken( txState, tokenName, tokenId );
			  try
			  {
					  using ( StorageReader statement = storageEngine.NewReader() )
					  {
						storageEngine.CreateCommands( commands, txState, statement, Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceLocker_Fields.None, long.MaxValue, NO_DECORATION );
					  }
			  }
			  catch ( Exception e ) when ( e is CreateConstraintFailureException || e is TransactionFailureException || e is ConstraintValidationException )
			  {
					throw new Exception( "Unable to create token '" + tokenName + "'", e );
			  }

			  return ReplicatedTokenRequestSerializer.CommandBytes( commands );
		 }
	}

}