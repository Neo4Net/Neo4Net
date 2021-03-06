﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using Rule = org.junit.Rule;

	using UncloseableDelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

	public class EmbeddedConfiguredAuthScenariosInteractionIT : ConfiguredAuthScenariosInteractionTestBase<EnterpriseLoginContext>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected NeoInteractionLevel<org.neo4j.kernel.enterprise.api.security.EnterpriseLoginContext> setUpNeoServer(java.util.Map<String, String> config) throws Throwable
		 protected internal override NeoInteractionLevel<EnterpriseLoginContext> setUpNeoServer( IDictionary<string, string> config )
		 {
			  return new EmbeddedInteraction( config, () => new UncloseableDelegatingFileSystemAbstraction(FileSystemRule.get()) );
		 }
	}

}