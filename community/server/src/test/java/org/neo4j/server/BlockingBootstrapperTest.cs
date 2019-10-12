using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Server
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class BlockingBootstrapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory homeDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory HomeDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockUntilStoppedIfTheWrappedStartIsSuccessful() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockUntilStoppedIfTheWrappedStartIsSuccessful()
		 {
			  AtomicInteger status = new AtomicInteger();
			  AtomicBoolean exited = new AtomicBoolean( false );
			  AtomicBoolean running = new AtomicBoolean( false );

			  BlockingBootstrapper bootstrapper = new BlockingBootstrapper( new BootstrapperAnonymousInnerClass( this, running ) );

			  (new Thread(() =>
			  {
				status.set( bootstrapper.Start( HomeDir.directory( "home-dir" ), null, java.util.Collections.emptyMap() ) );
				exited.set( true );
			  })).Start();

			  assertEventually( "Wrapped was not started", running.get, @is( true ), 10, TimeUnit.SECONDS );
			  assertThat( "Bootstrapper exited early", exited.get(), @is(false) );

			  bootstrapper.Stop();

			  assertEventually( "Wrapped was not stopped", running.get, @is( false ), 10, TimeUnit.SECONDS );
			  assertEventually( "Bootstrapper did not exit", exited.get, @is( true ), 10, TimeUnit.SECONDS );
			  assertThat( "Bootstrapper did not propagate exit status", status.get(), @is(0) );
		 }

		 private class BootstrapperAnonymousInnerClass : Bootstrapper
		 {
			 private readonly BlockingBootstrapperTest _outerInstance;

			 private AtomicBoolean _running;

			 public BootstrapperAnonymousInnerClass( BlockingBootstrapperTest outerInstance, AtomicBoolean running )
			 {
				 this.outerInstance = outerInstance;
				 this._running = running;
			 }

			 public int start( File homeDir, Optional<File> configFile, IDictionary<string, string> configOverrides )
			 {
				  _running.set( true );
				  return 0;
			 }

			 public int stop()
			 {
				  _running.set( false );
				  return 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBlockIfTheWrappedStartIsUnsuccessful() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBlockIfTheWrappedStartIsUnsuccessful()
		 {
			  AtomicInteger status = new AtomicInteger();
			  AtomicBoolean exited = new AtomicBoolean( false );

			  BlockingBootstrapper bootstrapper = new BlockingBootstrapper( new BootstrapperAnonymousInnerClass2( this ) );

			  (new Thread(() =>
			  {
				status.set( bootstrapper.Start( HomeDir.directory( "home-dir" ), null, java.util.Collections.emptyMap() ) );
				exited.set( true );
			  })).Start();

			  assertEventually( "Blocked unexpectedly", exited.get, @is( true ), 10, TimeUnit.SECONDS );
			  assertThat( "Bootstrapper did not propagate exit status", status.get(), @is(1) );
		 }

		 private class BootstrapperAnonymousInnerClass2 : Bootstrapper
		 {
			 private readonly BlockingBootstrapperTest _outerInstance;

			 public BootstrapperAnonymousInnerClass2( BlockingBootstrapperTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int start( File homeDir, Optional<File> configFile, IDictionary<string, string> configOverrides )
			 {
				  return 1;
			 }

			 public int stop()
			 {
				  return 0;
			 }
		 }
	}

}