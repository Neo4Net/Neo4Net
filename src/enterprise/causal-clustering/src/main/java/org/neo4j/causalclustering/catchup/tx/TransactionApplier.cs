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
namespace Neo4Net.causalclustering.catchup.tx
{
	using Neo4Net.com;
	using TransactionStream = Neo4Net.com.TransactionStream;
	using Neo4Net.com;
	using TransactionObligationFulfiller = Neo4Net.com.storecopy.TransactionObligationFulfiller;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// Receives and unpacks <seealso cref="Response responses"/>.
	/// Transaction obligations are handled by <seealso cref="TransactionObligationFulfiller"/> and
	/// <seealso cref="TransactionStream transaction streams"/> are <seealso cref="TransactionCommitProcess committed to the store"/>,
	/// in batches.
	/// <p/>
	/// It is assumed that any <seealso cref="TransactionStreamResponse response carrying transaction data"/> comes from the one
	/// and same thread.
	/// </summary>
	public class TransactionApplier
	{
		 private readonly TransactionRepresentationCommitProcess _commitProcess;
		 private readonly VersionContextSupplier _versionContextSupplier;

		 public TransactionApplier( DependencyResolver resolver )
		 {
			  _commitProcess = new TransactionRepresentationCommitProcess( resolver.ResolveDependency( typeof( TransactionAppender ) ), resolver.ResolveDependency( typeof( StorageEngine ) ) );
			  _versionContextSupplier = resolver.ResolveDependency( typeof( VersionContextSupplier ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void appendToLogAndApplyToStore(org.Neo4Net.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public virtual void AppendToLogAndApplyToStore( CommittedTransactionRepresentation tx )
		 {
			  _commitProcess.commit( new TransactionToApply( tx.TransactionRepresentation, tx.CommitEntry.TxId, _versionContextSupplier.VersionContext ), NULL, EXTERNAL );
		 }
	}

}