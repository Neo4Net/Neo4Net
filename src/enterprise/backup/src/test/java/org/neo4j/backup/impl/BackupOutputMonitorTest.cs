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
namespace Neo4Net.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using ParameterisedOutsideWorld = Neo4Net.CommandLine.Admin.ParameterisedOutsideWorld;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class BackupOutputMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
		 private readonly Monitors _monitors = new Monitors();
		 private OutsideWorld _outsideWorld;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _outsideWorld = new ParameterisedOutsideWorld( System.console(), System.out, System.err, System.in, new DefaultFileSystemAbstraction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void receivingStoreFilesMessageCorrect()
		 public virtual void ReceivingStoreFilesMessageCorrect()
		 {
			  // given
			  _monitors.addMonitorListener( new BackupOutputMonitor( _outsideWorld ) );

			  // when
			  StoreCopyClientMonitor storeCopyClientMonitor = _monitors.newMonitor( typeof( StoreCopyClientMonitor ) );
			  storeCopyClientMonitor.StartReceivingStoreFiles();

			  // then
			  assertTrue( SuppressOutput.OutputVoice.ToString().ToString().Contains("Start receiving store files") );
		 }
	}

}