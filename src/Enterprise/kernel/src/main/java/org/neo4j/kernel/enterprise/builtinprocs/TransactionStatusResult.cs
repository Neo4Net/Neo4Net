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
namespace Neo4Net.Kernel.enterprise.builtinprocs
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using TransactionExecutionStatistic = Neo4Net.Kernel.Impl.Api.TransactionExecutionStatistic;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.QueryId.ofInternalId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class TransactionStatusResult
	public class TransactionStatusResult
	{
		 private const string RUNNING_STATE = "Running";
		 private const string TERMINATED_STATE = "Terminated with reason: %s";

		 public readonly string TransactionId;
		 public readonly string Username;
		 public readonly IDictionary<string, object> MetaData;
		 public readonly string StartTime;
		 public readonly string Protocol;
		 public readonly string ClientAddress;
		 public readonly string RequestUri;

		 public readonly string CurrentQueryId;
		 public readonly string CurrentQuery;

		 public readonly long ActiveLockCount;
		 public readonly string Status;
		 public IDictionary<string, object> ResourceInformation;

		 public readonly long ElapsedTimeMillis;
		 public readonly long? CpuTimeMillis;
		 public readonly long WaitTimeMillis;
		 public readonly long? IdleTimeMillis;
		 public readonly long? AllocatedBytes;
		 public readonly long? AllocatedDirectBytes;
		 public readonly long PageHits;
		 public readonly long PageFaults;
		 /// <summary>
		 /// @since Neo4j 3.5 </summary>
		 public readonly string ConnectionId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionStatusResult(org.neo4j.kernel.api.KernelTransactionHandle transaction, TransactionDependenciesResolver transactionDependenciesResolver, java.util.Map<org.neo4j.kernel.api.KernelTransactionHandle,java.util.List<org.neo4j.kernel.api.query.QuerySnapshot>> handleSnapshotsMap, java.time.ZoneId zoneId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public TransactionStatusResult( KernelTransactionHandle transaction, TransactionDependenciesResolver transactionDependenciesResolver, IDictionary<KernelTransactionHandle, IList<QuerySnapshot>> handleSnapshotsMap, ZoneId zoneId )
		 {
			  this.TransactionId = transaction.UserTransactionName;
			  this.Username = transaction.Subject().username();
			  this.StartTime = ProceduresTimeFormatHelper.FormatTime( transaction.StartTime(), zoneId );
			  Optional<Status> terminationReason = transaction.TerminationReason();
			  this.ActiveLockCount = transaction.ActiveLocks().count();
			  IList<QuerySnapshot> querySnapshots = handleSnapshotsMap[transaction];
			  TransactionExecutionStatistic statistic = transaction.TransactionStatistic();
			  ElapsedTimeMillis = statistic.ElapsedTimeMillis;
			  CpuTimeMillis = statistic.CpuTimeMillis;
			  AllocatedBytes = statistic.HeapAllocatedBytes;
			  AllocatedDirectBytes = statistic.DirectAllocatedBytes;
			  WaitTimeMillis = statistic.WaitTimeMillis;
			  IdleTimeMillis = statistic.IdleTimeMillis;
			  PageHits = statistic.PageHits;
			  PageFaults = statistic.PageFaults;

			  if ( querySnapshots.Count > 0 )
			  {
					QuerySnapshot snapshot = querySnapshots[0];
					ClientConnectionInfo clientConnectionInfo = snapshot.ClientConnection();
					this.CurrentQueryId = ofInternalId( snapshot.InternalQueryId() ).ToString();
					this.CurrentQuery = snapshot.QueryText();
					this.Protocol = clientConnectionInfo.Protocol();
					this.ClientAddress = clientConnectionInfo.ClientAddress();
					this.RequestUri = clientConnectionInfo.RequestURI();
					this.ConnectionId = clientConnectionInfo.ConnectionId();
			  }
			  else
			  {
					this.CurrentQueryId = StringUtils.EMPTY;
					this.CurrentQuery = StringUtils.EMPTY;
					this.Protocol = StringUtils.EMPTY;
					this.ClientAddress = StringUtils.EMPTY;
					this.RequestUri = StringUtils.EMPTY;
					this.ConnectionId = StringUtils.EMPTY;
			  }
			  this.ResourceInformation = transactionDependenciesResolver.DescribeBlockingLocks( transaction );
			  this.Status = GetStatus( transaction, terminationReason, transactionDependenciesResolver );
			  this.MetaData = transaction.MetaData;
		 }

		 private string GetStatus( KernelTransactionHandle handle, Optional<Status> terminationReason, TransactionDependenciesResolver transactionDependenciesResolver )
		 {
			  return terminationReason.map( reason => format( TERMINATED_STATE, reason.code() ) ).orElseGet(() => GetExecutingStatus(handle, transactionDependenciesResolver));
		 }

		 private string GetExecutingStatus( KernelTransactionHandle handle, TransactionDependenciesResolver transactionDependenciesResolver )
		 {
			  return transactionDependenciesResolver.IsBlocked( handle ) ? "Blocked by: " + transactionDependenciesResolver.DescribeBlockingTransactions( handle ) : RUNNING_STATE;
		 }
	}

}