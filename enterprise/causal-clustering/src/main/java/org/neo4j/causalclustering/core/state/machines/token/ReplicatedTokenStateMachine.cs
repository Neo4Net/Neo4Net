using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.core.state.machines.token
{

	using Org.Neo4j.causalclustering.core.state.machines;
	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using VersionContext = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using TokenRegistry = Org.Neo4j.Kernel.impl.core.TokenRegistry;
	using LockGroup = Org.Neo4j.Kernel.impl.locking.LockGroup;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using NoSuchEntryException = Org.Neo4j.Kernel.impl.util.collection.NoSuchEntryException;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.encodeLogIndexAsTxHeader;

	public class ReplicatedTokenStateMachine : StateMachine<ReplicatedTokenRequest>
	{
		 private TransactionCommitProcess _commitProcess;

		 private readonly TokenRegistry _tokenRegistry;
		 private readonly VersionContext _versionContext;

		 private readonly Log _log;
		 private long _lastCommittedIndex = -1;

		 public ReplicatedTokenStateMachine( TokenRegistry tokenRegistry, LogProvider logProvider, VersionContextSupplier versionContextSupplier )
		 {
			  this._tokenRegistry = tokenRegistry;
			  this._versionContext = versionContextSupplier.VersionContext;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public virtual void InstallCommitProcess( TransactionCommitProcess commitProcess, long lastCommittedIndex )
		 {
			 lock ( this )
			 {
				  this._commitProcess = commitProcess;
				  this._lastCommittedIndex = lastCommittedIndex;
				  _log.info( format( "(%s) Updated lastCommittedIndex to %d", _tokenRegistry.TokenType, lastCommittedIndex ) );
			 }
		 }

		 public override void ApplyCommand( ReplicatedTokenRequest tokenRequest, long commandIndex, System.Action<Result> callback )
		 {
			 lock ( this )
			 {
				  if ( commandIndex <= _lastCommittedIndex )
				  {
						return;
				  }
      
				  int? tokenId = _tokenRegistry.getId( tokenRequest.TokenName() );
      
				  if ( tokenId == null )
				  {
						try
						{
							 ICollection<StorageCommand> commands = ReplicatedTokenRequestSerializer.ExtractCommands( tokenRequest.CommandBytes() );
							 tokenId = ApplyToStore( commands, commandIndex );
						}
						catch ( NoSuchEntryException )
						{
							 throw new System.InvalidOperationException( "Commands did not contain token command" );
						}
      
						_tokenRegistry.put( new NamedToken( tokenRequest.TokenName(), tokenId.Value ) );
				  }
      
				  callback( Result.of( tokenId ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int applyToStore(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> commands, long logIndex) throws org.neo4j.kernel.impl.util.collection.NoSuchEntryException
		 private int ApplyToStore( ICollection<StorageCommand> commands, long logIndex )
		 {
			  int tokenId = ExtractTokenId( commands );

			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( commands );
			  representation.SetHeader( encodeLogIndexAsTxHeader( logIndex ), 0, 0, 0, 0L, 0L, 0 );

			  try
			  {
					  using ( LockGroup ignored = new LockGroup() )
					  {
						_commitProcess.commit( new TransactionToApply( representation, _versionContext ), CommitEvent.NULL, TransactionApplicationMode.EXTERNAL );
					  }
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }

			  return tokenId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int extractTokenId(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> commands) throws org.neo4j.kernel.impl.util.collection.NoSuchEntryException
		 private int ExtractTokenId( ICollection<StorageCommand> commands )
		 {
			  foreach ( StorageCommand command in commands )
			  {
					if ( command is Command.TokenCommand )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return ((org.neo4j.kernel.impl.transaction.command.Command.TokenCommand<? extends org.neo4j.kernel.impl.store.record.TokenRecord>) command).getAfter().getIntId();
						 return ( ( Command.TokenCommand<TokenRecord> ) command ).After.IntId;
					}
			  }
			  throw new NoSuchEntryException( "Expected command not found" );
		 }

		 public override void Flush()
		 {
			 lock ( this )
			 {
				  // already implicitly flushed to the store
			 }
		 }

		 public override long LastAppliedIndex()
		 {
			  if ( _commitProcess == null )
			  {
					/// <summary>
					/// See <seealso cref="installCommitProcess"/>. </summary>
					throw new System.InvalidOperationException( "Value has not been installed" );
			  }
			  return _lastCommittedIndex;
		 }
	}

}