using System.Collections.Generic;

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
namespace Neo4Net.Test.matchers
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;

	public sealed class Matchers
	{
		 private Matchers()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super java.util.List<org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage>> hasMessage(org.neo4j.causalclustering.core.consensus.RaftMessages_BaseRaftMessage message)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> HasMessage( Neo4Net.causalclustering.core.consensus.RaftMessages_BaseRaftMessage message )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( message );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>
		 {
			 private Neo4Net.causalclustering.core.consensus.RaftMessages_BaseRaftMessage _message;

			 public TypeSafeMatcherAnonymousInnerClass( Neo4Net.causalclustering.core.consensus.RaftMessages_BaseRaftMessage message )
			 {
				 this._message = message;
			 }

			 protected internal override bool matchesSafely( IList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> messages )
			 {
				  return messages.Contains( _message );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "has message " + _message );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super java.util.List<org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage>> hasRaftLogEntries(java.util.Collection<org.neo4j.causalclustering.core.consensus.log.RaftLogEntry> expectedEntries)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> HasRaftLogEntries( ICollection<RaftLogEntry> expectedEntries )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( expectedEntries );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<IList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>
		 {
			 private ICollection<RaftLogEntry> _expectedEntries;

			 public TypeSafeMatcherAnonymousInnerClass2( ICollection<RaftLogEntry> expectedEntries )
			 {
				 this._expectedEntries = expectedEntries;
			 }

			 protected internal override bool matchesSafely( IList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> messages )
			 {
				  IList<RaftLogEntry> entries = messages.Where( message => message is Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ).Select( m => ( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) m ).flatMap( x => Arrays.stream( x.entries() ) ).ToList();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
				  return entries.containsAll( _expectedEntries );

			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "log entries " + _expectedEntries );
			 }
		 }
	}

}