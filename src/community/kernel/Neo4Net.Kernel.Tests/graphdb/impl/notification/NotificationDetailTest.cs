using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.GraphDb.impl.notification
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	internal class NotificationDetailTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructIndexDetails()
		 internal virtual void ShouldConstructIndexDetails()
		 {
			  NotificationDetail detail = NotificationDetail_Factory.Index( "Person", "name" );

			  assertThat( detail.Name(), equalTo("hinted index") );
			  assertThat( detail.Value(), equalTo("index on :Person(name)") );
			  assertThat( detail.ToString(), equalTo("hinted index is: index on :Person(name)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructSuboptimalIndexDetails()
		 internal virtual void ShouldConstructSuboptimalIndexDetails()
		 {
			  NotificationDetail detail = NotificationDetail_Factory.SuboptimalIndex( "Person", "name" );

			  assertThat( detail.Name(), equalTo("index") );
			  assertThat( detail.Value(), equalTo("index on :Person(name)") );
			  assertThat( detail.ToString(), equalTo("index is: index on :Person(name)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructCartesianProductDetailsSingular()
		 internal virtual void ShouldConstructCartesianProductDetailsSingular()
		 {
			  ISet<string> idents = new HashSet<string>();
			  idents.Add( "n" );
			  NotificationDetail detail = NotificationDetail_Factory.CartesianProduct( idents );

			  assertThat( detail.Name(), equalTo("identifier") );
			  assertThat( detail.Value(), equalTo("(n)") );
			  assertThat( detail.ToString(), equalTo("identifier is: (n)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructCartesianProductDetails()
		 internal virtual void ShouldConstructCartesianProductDetails()
		 {
			  ISet<string> idents = new SortedSet<string>();
			  idents.Add( "n" );
			  idents.Add( "node2" );
			  NotificationDetail detail = NotificationDetail_Factory.CartesianProduct( idents );

			  assertThat( detail.Name(), equalTo("identifiers") );
			  assertThat( detail.Value(), equalTo("(n, node2)") );
			  assertThat( detail.ToString(), equalTo("identifiers are: (n, node2)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructJoinHintDetailsSingular()
		 internal virtual void ShouldConstructJoinHintDetailsSingular()
		 {
			  IList<string> idents = new List<string>();
			  idents.Add( "n" );
			  NotificationDetail detail = NotificationDetail_Factory.JoinKey( idents );

			  assertThat( detail.Name(), equalTo("hinted join key identifier") );
			  assertThat( detail.Value(), equalTo("n") );
			  assertThat( detail.ToString(), equalTo("hinted join key identifier is: n") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructJoinHintDetails()
		 internal virtual void ShouldConstructJoinHintDetails()
		 {
			  IList<string> idents = new List<string>();
			  idents.Add( "n" );
			  idents.Add( "node2" );
			  NotificationDetail detail = NotificationDetail_Factory.JoinKey( idents );

			  assertThat( detail.Name(), equalTo("hinted join key identifiers") );
			  assertThat( detail.Value(), equalTo("n, node2") );
			  assertThat( detail.ToString(), equalTo("hinted join key identifiers are: n, node2") );
		 }
	}

}