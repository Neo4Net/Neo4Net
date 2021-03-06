﻿/*
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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Condition = Org.Neo4j.Io.pagecache.stress.Condition;
	using PageCacheStressTest = Org.Neo4j.Io.pagecache.stress.PageCacheStressTest;
	using DefaultPageCacheTracer = Org.Neo4j.Io.pagecache.tracing.DefaultPageCacheTracer;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.stress.Conditions.numberOfEvictions;

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