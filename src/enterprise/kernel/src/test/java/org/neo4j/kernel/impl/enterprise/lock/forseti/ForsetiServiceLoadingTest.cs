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
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using CommunityLockManger = Neo4Net.Kernel.impl.locking.community.CommunityLockManger;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;

	public class ForsetiServiceLoadingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new Neo4Net.test.rule.EmbeddedDatabaseRule().startLazily();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseForsetiAsDefaultLockManager()
		 public virtual void ShouldUseForsetiAsDefaultLockManager()
		 {
			  // When
			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;

			  // Then
			  assertThat( Db.DependencyResolver.resolveDependency( typeof( Locks ) ), instanceOf( typeof( ForsetiLockManager ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowUsingCommunityLockManager()
		 public virtual void ShouldAllowUsingCommunityLockManager()
		 {
			  // When
			  DbRule.withSetting( GraphDatabaseSettings.lock_manager, "community" );
			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;

			  // Then
			  assertThat( Db.DependencyResolver.resolveDependency( typeof( Locks ) ), instanceOf( typeof( CommunityLockManger ) ) );
		 }
	}

}