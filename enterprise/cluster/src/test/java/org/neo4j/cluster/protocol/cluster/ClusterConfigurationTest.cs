using System;
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
namespace Org.Neo4j.cluster.protocol.cluster
{
	using Test = org.junit.Test;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.IterableMatcher.matchesIterable;

	public class ClusterConfigurationTest
	{
		 public static URI Neo4jServer1Uri;
		 public static InstanceId Neo4jServerId;

		 static ClusterConfigurationTest()
		 {
			  try
			  {
					Neo4jServer1Uri = new URI( "neo4j://server1" );
					Neo4jServerId = new InstanceId( 1 );
			  }
			  catch ( URISyntaxException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
		 }

		 internal ClusterConfiguration Configuration = new ClusterConfiguration( "default", NullLogProvider.Instance, new List<URI>() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenEmptyClusterWhenNodeAddedThenNodeWasAdded()
		 public virtual void GivenEmptyClusterWhenNodeAddedThenNodeWasAdded()
		 {
			  Configuration.joined( Neo4jServerId, Neo4jServer1Uri );

			  assertThat( Configuration.MemberIds, matchesIterable( Iterables.iterable( Neo4jServerId ) ) );
			  assertThat( Configuration.getUriForId( Neo4jServerId ), equalTo( Neo4jServer1Uri ) );
			  assertThat( Configuration.MemberURIs, equalTo( Arrays.asList( Neo4jServer1Uri ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenEmptyClusterWhenNodeIsAddedTwiceThenNodeWasAddedOnce()
		 public virtual void GivenEmptyClusterWhenNodeIsAddedTwiceThenNodeWasAddedOnce()
		 {
			  Configuration.joined( Neo4jServerId, Neo4jServer1Uri );
			  Configuration.joined( Neo4jServerId, Neo4jServer1Uri );

			  assertThat( Configuration.MemberIds, matchesIterable( Iterables.iterable( Neo4jServerId ) ) );
			  assertThat( Configuration.getUriForId( Neo4jServerId ), equalTo( Neo4jServer1Uri ) );
			  assertThat( Configuration.MemberURIs, equalTo( Arrays.asList( Neo4jServer1Uri ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithOneNodeWhenNodeIsRemovedThenClusterIsEmpty()
		 public virtual void GivenClusterWithOneNodeWhenNodeIsRemovedThenClusterIsEmpty()
		 {
			  Configuration.joined( Neo4jServerId, Neo4jServer1Uri );
			  Configuration.left( Neo4jServerId );

			  assertThat( Configuration.MemberIds, matchesIterable( Iterables.empty() ) );
			  assertThat( Configuration.getUriForId( Neo4jServerId ), equalTo( null ) );
			  assertThat( Configuration.MemberURIs, equalTo( System.Linq.Enumerable.Empty<URI>() ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithOneNodeWhenNodeIsRemovedTwiceThenClusterIsEmpty()
		 public virtual void GivenClusterWithOneNodeWhenNodeIsRemovedTwiceThenClusterIsEmpty()
		 {
			  Configuration.joined( Neo4jServerId, Neo4jServer1Uri );
			  Configuration.left( Neo4jServerId );
			  Configuration.left( Neo4jServerId );

			  assertThat( Configuration.MemberIds, matchesIterable( Iterables.empty() ) );
			  assertThat( Configuration.getUriForId( Neo4jServerId ), equalTo( null ) );
			  assertThat( Configuration.MemberURIs, equalTo( System.Linq.Enumerable.Empty<URI>() ) );

		 }
	}


}