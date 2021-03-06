﻿/*
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
namespace Org.Neo4j.Kernel.ha.cluster.modeswitch
{

	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using Org.Neo4j.Kernel.ha;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using DefaultLabelIdCreator = Org.Neo4j.Kernel.impl.core.DefaultLabelIdCreator;
	using TokenCreator = Org.Neo4j.Kernel.impl.core.TokenCreator;

	public class LabelTokenCreatorSwitcher : AbstractComponentSwitcher<TokenCreator>
	{
		 private readonly DelegateInvocationHandler<Master> _master;
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly System.Func<Kernel> _kernelSupplier;

		 public LabelTokenCreatorSwitcher( DelegateInvocationHandler<TokenCreator> @delegate, DelegateInvocationHandler<Master> master, RequestContextFactory requestContextFactory, System.Func<Kernel> kernelSupplier ) : base( @delegate )
		 {
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
			  this._kernelSupplier = kernelSupplier;
		 }

		 protected internal override TokenCreator MasterImpl
		 {
			 get
			 {
				  return new DefaultLabelIdCreator( _kernelSupplier );
			 }
		 }

		 protected internal override TokenCreator SlaveImpl
		 {
			 get
			 {
				  return new SlaveLabelTokenCreator( _master.cement(), _requestContextFactory );
			 }
		 }
	}

}