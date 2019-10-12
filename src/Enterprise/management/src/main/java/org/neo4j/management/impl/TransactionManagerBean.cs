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
namespace Neo4Net.management.impl
{

	using Service = Neo4Net.Helpers.Service;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4jMBean = Neo4Net.Jmx.impl.Neo4jMBean;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class TransactionManagerBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	public sealed class TransactionManagerBean : ManagementBeanProvider
	{
		 public TransactionManagerBean() : base(typeof(TransactionManager))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new TransactionManagerImpl( management );
		 }

		 private class TransactionManagerImpl : Neo4jMBean, TransactionManager
		 {
			  internal readonly DatabaseTransactionStats TxMonitor;
			  internal readonly DataSourceManager DataSourceManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionManagerImpl(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
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