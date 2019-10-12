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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using Org.Neo4j.com;
	using TransactionStream = Org.Neo4j.com.TransactionStream;
	using Org.Neo4j.com;
	using TransactionObligationFulfiller = Org.Neo4j.com.storecopy.TransactionObligationFulfiller;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

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
//ORIGINAL LINE: public void appendToLogAndApplyToStore(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public virtual void AppendToLogAndApplyToStore( CommittedTransactionRepresentation tx )
		 {
			  _commitProcess.commit( new TransactionToApply( tx.TransactionRepresentation, tx.CommitEntry.TxId, _versionContextSupplier.VersionContext ), NULL, EXTERNAL );
		 }
	}

}