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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Condition = Neo4Net.Io.pagecache.stress.Condition;
	using PageCacheStressTest = Neo4Net.Io.pagecache.stress.PageCacheStressTest;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.stress.Conditions.numberOfEvictions;

	/// <summary>
	/// A stress test for Muninn page cache.
	/// 
	/// Uses @PageCacheStressTest - see details there.
	/// 
	/// Configured to run until it sees a million evictions, which should take few minutes.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class MuninnPageCacheStressIT
	internal class MuninnPageCacheStressIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTheStressOfManyManyEvictions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleTheStressOfManyManyEvictions()
		 {
			  DefaultPageCacheTracer monitor = new DefaultPageCacheTracer();
			  Condition condition = numberOfEvictions( monitor, 100_000 );

			  PageCacheStressTest runner = ( new PageCacheStressTest.Builder() ).withWorkingDirectory(TestDirectory.directory()).with(monitor).with(condition).build();

			  runner.Run();
		 }
	}

}