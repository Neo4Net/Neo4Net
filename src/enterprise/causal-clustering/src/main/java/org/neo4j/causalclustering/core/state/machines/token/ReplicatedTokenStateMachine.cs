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

	using Neo4Net.causalclustering.core.state.machines;
	using NamedToken = Neo4Net.Kernel.Api.Internal.NamedToken;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using NoSuchEntryException = Neo4Net.Kernel.impl.util.collection.NoSuchEntryException;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.encodeLogIndexAsTxHeader;

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
//ORIGINAL LINE: private int applyToStore(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StorageCommand> commands, long logIndex) throws Neo4Net.kernel.impl.util.collection.NoSuchEntryException
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
//ORIGINAL LINE: private int extractTokenId(java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StorageCommand> commands) throws Neo4Net.kernel.impl.util.collection.NoSuchEntryException
		 private int ExtractTokenId( ICollection<StorageCommand> commands )
		 {
			  foreach ( StorageCommand command in commands )
			  {
					if ( command is Command.TokenCommand )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return ((Neo4Net.kernel.impl.transaction.command.Command.TokenCommand<? extends Neo4Net.kernel.impl.store.record.TokenRecord>) command).getAfter().getIntId();
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