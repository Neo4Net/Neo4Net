using System.Collections.Generic;

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
namespace Neo4Net.metrics.output
{
	using Clock = com.codahale.metrics.Clock;
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RotatableCsvReporterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private RotatingFileOutputStreamSupplier _fileOutputStreamSupplier = mock( typeof( RotatingFileOutputStreamSupplier ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopAllWritersOnStop() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StopAllWritersOnStop()
		 {
			  Stream outputStream = mock( typeof( Stream ) );
			  when( _fileOutputStreamSupplier.get() ).thenReturn(outputStream);
			  RotatableCsvReporter reporter = new RotatableCsvReporter( mock( typeof( MetricRegistry ) ), Locale.US, TimeUnit.SECONDS, TimeUnit.SECONDS, Clock.defaultClock(), TestDirectory.directory(), (file, rotationListener) => _fileOutputStreamSupplier );
			  SortedDictionary<string, Gauge> gauges = new SortedDictionary<string, Gauge>();
			  gauges["a"] = () => ThreadLocalRandom.current().nextLong();
			  gauges["b"] = () => ThreadLocalRandom.current().nextLong();
			  gauges["c"] = () => ThreadLocalRandom.current().nextLong();
			  reporter.report( gauges, new SortedDictionary<>(), new SortedDictionary<>(), new SortedDictionary<>(), new SortedDictionary<>() );

			  gauges["b"] = () => ThreadLocalRandom.current().nextLong();
			  gauges["c"] = () => ThreadLocalRandom.current().nextLong();
			  gauges["d"] = () => ThreadLocalRandom.current().nextLong();
			  reporter.report( gauges, new SortedDictionary<>(), new SortedDictionary<>(), new SortedDictionary<>(), new SortedDictionary<>() );

			  reporter.Stop();
			  verify( _fileOutputStreamSupplier, times( 4 ) ).close();
		 }
	}

}