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
namespace Neo4Net.management.impl
{

	using Service = Neo4Net.Helpers.Service;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class TransactionManagerBean extends Neo4Net.jmx.impl.ManagementBeanProvider
	public sealed class TransactionManagerBean : ManagementBeanProvider
	{
		 public TransactionManagerBean() : base(typeof(TransactionManager))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4Net.jmx.impl.Neo4NetMBean createMBean(Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new TransactionManagerImpl( management );
		 }

		 private class TransactionManagerImpl : Neo4NetMBean, TransactionManager
		 {
			  internal readonly DatabaseTransactionStats TxMonitor;
			  internal readonly DataSourceManager DataSourceManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionManagerImpl(Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal TransactionManagerImpl( ManagementData management ) : base( management )
			  {
					this.TxMonitor = management.ResolveDependency( typeof( DatabaseTransactionStats ) );
					this.DataSourceManager = management.ResolveDependency( typeof( DataSourceManager ) );
			  }

			  public virtual long NumberOfOpenTransactions
			  {
				  get
				  {
						return TxMonitor.NumberOfActiveTransactions;
				  }
			  }

			  public virtual long PeakNumberOfConcurrentTransactions
			  {
				  get
				  {
						return TxMonitor.PeakConcurrentNumberOfTransactions;
				  }
			  }

			  public virtual long NumberOfOpenedTransactions
			  {
				  get
				  {
						return TxMonitor.NumberOfStartedTransactions;
				  }
			  }

			  public virtual long NumberOfCommittedTransactions
			  {
				  get
				  {
						return TxMonitor.NumberOfCommittedTransactions;
				  }
			  }

			  public virtual long NumberOfRolledBackTransactions
			  {
				  get
				  {
						return TxMonitor.NumberOfRolledBackTransactions;
				  }
			  }

			  public virtual long LastCommittedTxId
			  {
				  get
				  {
						NeoStoreDataSource neoStoreDataSource = DataSourceManager.DataSource;
						if ( neoStoreDataSource == null )
						{
							 return -1;
						}
						return neoStoreDataSource.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;
				  }
			  }
		 }
	}

}