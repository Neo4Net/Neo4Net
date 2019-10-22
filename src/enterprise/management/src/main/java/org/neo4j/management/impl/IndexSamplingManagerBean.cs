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

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Service = Neo4Net.Helpers.Service;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.TokenRead_Fields.NO_TOKEN;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public final class IndexSamplingManagerBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	public sealed class IndexSamplingManagerBean : ManagementBeanProvider
	{
		 public IndexSamplingManagerBean() : base(typeof(IndexSamplingManager))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.jmx.impl.Neo4NetMBean createMBean(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new IndexSamplingManagerImpl( management );
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
		 {
			  return new IndexSamplingManagerImpl( management, true );
		 }

		 private class IndexSamplingManagerImpl : Neo4NetMBean, IndexSamplingManager
		 {
			  internal readonly StoreAccess Access;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexSamplingManagerImpl(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal IndexSamplingManagerImpl( ManagementData management ) : base( management )
			  {
					this.Access = Access( management );
			  }

			  internal IndexSamplingManagerImpl( ManagementData management, bool mxBean ) : base( management, mxBean )
			  {
					this.Access = Access( management );
			  }

			  public override void TriggerIndexSampling( string labelKey, string propertyKey, bool forceSample )
			  {
					Access.triggerIndexSampling( labelKey, propertyKey, forceSample );
			  }
		 }

		 private static StoreAccess Access( ManagementData management )
		 {
			  StoreAccess access = new StoreAccess();
			  management.KernelData.DataSourceManager.addListener( access );
			  return access;
		 }

		 internal class StoreAccess : DataSourceManager.Listener
		 {
			  private class State
			  {
					internal readonly StorageEngine StorageEngine;
					internal readonly IndexingService IndexingService;
					internal readonly TokenHolders TokenHolders;

					internal State( StorageEngine storageEngine, IndexingService indexingService, TokenHolders tokenHolders )
					{
						 this.StorageEngine = storageEngine;
						 this.IndexingService = indexingService;
						 this.TokenHolders = tokenHolders;
					}
			  }
			  internal volatile State State;

			  public override void Registered( NeoStoreDataSource dataSource )
			  {
					DependencyResolver dependencyResolver = dataSource.DependencyResolver;
					State = new State( dependencyResolver.ResolveDependency( typeof( StorageEngine ) ), dependencyResolver.ResolveDependency( typeof( IndexingService ) ), dependencyResolver.ResolveDependency( typeof( TokenHolders ) ) );
			  }

			  public override void Unregistered( NeoStoreDataSource dataSource )
			  {
					State = null;
			  }

			  public virtual void TriggerIndexSampling( string labelKey, string propertyKey, bool forceSample )
			  {
					int labelKeyId = NO_TOKEN;
					int propertyKeyId = NO_TOKEN;
					State state = this.State;
					if ( state != null )
					{
						 labelKeyId = state.TokenHolders.labelTokens().getIdByName(labelKey);
						 propertyKeyId = state.TokenHolders.propertyKeyTokens().getIdByName(propertyKey);
					}
					if ( state == null || labelKeyId == NO_TOKEN || propertyKeyId == NO_TOKEN )
					{
						 throw new System.ArgumentException( "No property or label key was found associated with " + propertyKey + " and " + labelKey );
					}
					try
					{
						 state.IndexingService.triggerIndexSampling( SchemaDescriptorFactory.forLabel( labelKeyId, propertyKeyId ), GetIndexSamplingMode( forceSample ) );
					}
					catch ( IndexNotFoundKernelException e )
					{
						 throw new System.ArgumentException( e.Message );
					}
			  }

			  internal virtual IndexSamplingMode GetIndexSamplingMode( bool forceSample )
			  {
					if ( forceSample )
					{
						 return IndexSamplingMode.TRIGGER_REBUILD_ALL;
					}
					else
					{
						 return IndexSamplingMode.TRIGGER_REBUILD_UPDATED;
					}
			  }
		 }
	}

}