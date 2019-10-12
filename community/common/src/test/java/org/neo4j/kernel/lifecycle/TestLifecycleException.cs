using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Lifecycle
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.SHUTDOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.STARTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.STOPPED;

	internal class TestLifecycleException
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMakeNoneToStoppedIntoHumanReadableInitMessage()
		 internal virtual void ShouldMakeNoneToStoppedIntoHumanReadableInitMessage()
		 {
			  assertThat( ExceptionFor( NONE, STOPPED ).Message, @is( "Component 'SomeComponent' failed to initialize." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMakeStoppedToStartedIntoHumanReadableStartingMessage()
		 internal virtual void ShouldMakeStoppedToStartedIntoHumanReadableStartingMessage()
		 {
			  assertThat( ExceptionFor( STOPPED, STARTED ).Message, @is( "Component 'SomeComponent' was successfully initialized, but failed to start." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMakeStartedToStoppedIntoHumanReadableStoppingMessage()
		 internal virtual void ShouldMakeStartedToStoppedIntoHumanReadableStoppingMessage()
		 {
			  assertThat( ExceptionFor( STARTED, STOPPED ).Message, @is( "Component 'SomeComponent' failed to stop." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMakeShutdownIntoHumanReadableShutdownMessage()
		 internal virtual void ShouldMakeShutdownIntoHumanReadableShutdownMessage()
		 {
			  assertThat( ExceptionFor( STOPPED, SHUTDOWN ).Message, @is( "Component 'SomeComponent' failed to shut down." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeRootCauseMessageInExceptionMessage()
		 internal virtual void ShouldIncludeRootCauseMessageInExceptionMessage()
		 {
			  Exception root = new Exception( "big bad root cause" );
			  Exception intermediate = new Exception( "intermediate exception", root );
			  assertThat( ExceptionFor( STARTED, STOPPED, intermediate ).Message, containsString( root.Message ) );
		 }

		 private LifecycleException ExceptionFor( LifecycleStatus from, LifecycleStatus to )
		 {
			  return ExceptionFor( from, to, null );
		 }

		 private LifecycleException ExceptionFor( LifecycleStatus from, LifecycleStatus to, Exception cause )
		 {
			  return new LifecycleException(new ObjectAnonymousInnerClass(this)
			 , from, to, cause);
		 }

		 private class ObjectAnonymousInnerClass : object
		 {
			 private readonly TestLifecycleException _outerInstance;

			 public ObjectAnonymousInnerClass( TestLifecycleException outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override string ToString()
			 {
				  return "SomeComponent";
			 }
		 }

	}

}