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
namespace Neo4Net.Kernel.enterprise.api.security.provider
{
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Neo4Net.Kernel.api.security.UserManagerSupplier;
	using SecurityProvider = Neo4Net.Kernel.api.security.provider.SecurityProvider;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class EnterpriseNoAuthSecurityProvider : LifecycleAdapter, SecurityProvider
	{
		 public static readonly EnterpriseNoAuthSecurityProvider Instance = new EnterpriseNoAuthSecurityProvider();

		 private EnterpriseNoAuthSecurityProvider()
		 {
		 }

		 public override AuthManager AuthManager()
		 {
			  return EnterpriseAuthManager.NO_AUTH;
		 }

		 public override UserManagerSupplier UserManagerSupplier()
		 {
			  return UserManagerSupplier.NO_AUTH;
		 }
	}

}