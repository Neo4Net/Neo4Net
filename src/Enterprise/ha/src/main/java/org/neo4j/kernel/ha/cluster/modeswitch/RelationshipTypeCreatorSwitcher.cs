/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{

	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using DefaultRelationshipTypeCreator = Neo4Net.Kernel.impl.core.DefaultRelationshipTypeCreator;
	using TokenCreator = Neo4Net.Kernel.impl.core.TokenCreator;

	public class RelationshipTypeCreatorSwitcher : AbstractComponentSwitcher<TokenCreator>
	{
		 private readonly DelegateInvocationHandler<Master> _master;
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly System.Func<Kernel> _kernelSupplier;

		 public RelationshipTypeCreatorSwitcher( DelegateInvocationHandler<TokenCreator> @delegate, DelegateInvocationHandler<Master> master, RequestContextFactory requestContextFactory, System.Func<Kernel> kernelSupplier ) : base( @delegate )
		 {
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
			  this._kernelSupplier = kernelSupplier;
		 }

		 protected internal override TokenCreator MasterImpl
		 {
			 get
			 {
				  return new DefaultRelationshipTypeCreator( _kernelSupplier );
			 }
		 }

		 protected internal override TokenCreator SlaveImpl
		 {
			 get
			 {
				  return new SlaveRelationshipTypeCreator( _master.cement(), _requestContextFactory );
			 }
		 }
	}

}