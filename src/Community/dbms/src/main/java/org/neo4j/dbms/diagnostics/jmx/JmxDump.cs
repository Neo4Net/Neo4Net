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
namespace Neo4Net.Dbms.diagnostics.jmx
{
	using HotSpotDiagnosticMXBean = com.sun.management.HotSpotDiagnosticMXBean;


	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using DiagnosticsReportSources = Neo4Net.Diagnostics.DiagnosticsReportSources;
	using DiagnosticsReporterProgress = Neo4Net.Diagnostics.DiagnosticsReporterProgress;
	using ProgressAwareInputStream = Neo4Net.Diagnostics.ProgressAwareInputStream;
	using DumpUtils = Neo4Net.Diagnostics.utils.DumpUtils;

	/// <summary>
	/// Encapsulates remoting functionality for collecting diagnostics information on running instances.
	/// </summary>
	public class JmxDump
	{
		 private readonly MBeanServerConnection _mBeanServer;
		 private Properties _systemProperties;

		 private JmxDump( MBeanServerConnection mBeanServer )
		 {
			  this._mBeanServer = mBeanServer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static JmxDump connectTo(String jmxAddress) throws java.io.IOException
		 public static JmxDump ConnectTo( string jmxAddress )
		 {
			  JMXServiceURL url = new JMXServiceURL( jmxAddress );
			  JMXConnector connect = JMXConnectorFactory.connect( url );

			  return new JmxDump( connect.MBeanServerConnection );
		 }

		 public virtual void AttachSystemProperties( Properties systemProperties )
		 {
			  this._systemProperties = systemProperties;
		 }

		 /// <summary>
		 /// Captures a thread dump of the running instance.
		 /// </summary>
		 /// <returns> a diagnostics source the will emit a thread dump. </returns>
		 public virtual DiagnosticsReportSource ThreadDump()
		 {
			  return DiagnosticsReportSources.newDiagnosticsString("threaddump.txt", () =>
			  {
				string result;
				try
				{
					 // Try to invoke real thread dump
					 result = ( string ) _mBeanServer.invoke( new ObjectName( "com.sun.management:type=DiagnosticCommand" ), "threadPrint", new object[]{ null }, new string[]{ typeof( string[] ).FullName } );
				}
				catch ( Exception exception ) when ( exception is InstanceNotFoundException || exception is ReflectionException || exception is MBeanException || exception is MalformedObjectNameException || exception is IOException )
				{
					 // Failed, do a poor mans attempt
					 result = MXThreadDump;
				}

				return result;
			  });
		 }

		 /// <summary>
		 /// If "DiagnosticCommand" bean isn't available, for reasons unknown, try our best to reproduce the output. For obvious
		 /// reasons we can't get everything, since it's not exposed in java.
		 /// </summary>
		 /// <returns> a thread dump. </returns>
		 private string MXThreadDump
		 {
			 get
			 {
				  ThreadMXBean threadMxBean;
				  try
				  {
						threadMxBean = ManagementFactory.getPlatformMXBean( _mBeanServer, typeof( ThreadMXBean ) );
				  }
				  catch ( IOException )
				  {
						return "ERROR: Unable to produce any thread dump";
				  }
   
				  return DumpUtils.threadDump( threadMxBean, _systemProperties );
			 }
		 }

		 public virtual DiagnosticsReportSource HeapDump()
		 {
			  return new DiagnosticsReportSourceAnonymousInnerClass( this );
		 }

		 private class DiagnosticsReportSourceAnonymousInnerClass : DiagnosticsReportSource
		 {
			 private readonly JmxDump _outerInstance;

			 public DiagnosticsReportSourceAnonymousInnerClass( JmxDump outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public string destinationPath()
			 {
				  return "heapdump.hprof";
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addToArchive(java.nio.file.Path archiveDestination, org.neo4j.diagnostics.DiagnosticsReporterProgress progress) throws java.io.IOException
			 public void addToArchive( Path archiveDestination, DiagnosticsReporterProgress progress )
			 {
				  // Heap dump has to target an actual file, we cannot stream directly to the archive
				  progress.Info( "dumping..." );
				  Path tempFile = Files.createTempFile( "neo4j-heapdump", ".hprof" );
				  Files.deleteIfExists( tempFile );
				  outerInstance.heapDump( tempFile.toAbsolutePath().ToString() );

				  // Track progress of archiving process
				  progress.Info( "archiving..." );
				  long size = Files.size( tempFile );
				  Stream @in = Files.newInputStream( tempFile );
				  using ( ProgressAwareInputStream inStream = new ProgressAwareInputStream( @in, size, progress.percentChanged ) )
				  {
						Files.copy( inStream, archiveDestination );
				  }

				  Files.delete( tempFile );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long estimatedSize(org.neo4j.diagnostics.DiagnosticsReporterProgress progress) throws java.io.IOException
			 public long estimatedSize( DiagnosticsReporterProgress progress )
			 {
				  MemoryMXBean bean = ManagementFactory.getPlatformMXBean( _outerInstance.mBeanServer, typeof( MemoryMXBean ) );
				  long totalMemory = bean.HeapMemoryUsage.Committed + bean.NonHeapMemoryUsage.Committed;

				  // We first write raw to disk then write to archive, 5x compression is a reasonable worst case estimation
				  return ( long )( totalMemory * 1.2 );
			 }
		 }

		 /// <param name="destination"> file path to send heap dump to, has to end with ".hprof" </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void heapDump(String destination) throws java.io.IOException
		 private void HeapDump( string destination )
		 {
			  HotSpotDiagnosticMXBean hotSpotDiagnosticMXBean = ManagementFactory.getPlatformMXBean( _mBeanServer, typeof( HotSpotDiagnosticMXBean ) );
			  hotSpotDiagnosticMXBean.dumpHeap( destination, false );
		 }

		 public virtual DiagnosticsReportSource SystemProperties()
		 {
			  return new DiagnosticsReportSourceAnonymousInnerClass2( this );
		 }

		 private class DiagnosticsReportSourceAnonymousInnerClass2 : DiagnosticsReportSource
		 {
			 private readonly JmxDump _outerInstance;

			 public DiagnosticsReportSourceAnonymousInnerClass2( JmxDump outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public string destinationPath()
			 {
				  return "vm.prop";
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addToArchive(java.nio.file.Path archiveDestination, org.neo4j.diagnostics.DiagnosticsReporterProgress progress) throws java.io.IOException
			 public void addToArchive( Path archiveDestination, DiagnosticsReporterProgress progress )
			 {
				  using ( PrintStream printStream = new PrintStream( Files.newOutputStream( archiveDestination ) ) )
				  {
						_outerInstance.systemProperties.list( printStream );
				  }
			 }

			 public long estimatedSize( DiagnosticsReporterProgress progress )
			 {
				  return 0;
			 }
		 }

		 public virtual DiagnosticsReportSource EnvironmentVariables()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return NewReportsBeanSource( "env.prop", Reports::getEnvironmentVariables );
		 }

		 public virtual DiagnosticsReportSource ListTransactions()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return NewReportsBeanSource( "listTransactions.txt", Reports::listTransactions );
		 }

		 private DiagnosticsReportSource NewReportsBeanSource( string destination, ReportsInvoker reportsInvoker )
		 {
			  return DiagnosticsReportSources.newDiagnosticsString(destination, () =>
			  {
				try
				{
					 ObjectName name = new ObjectName( "org.neo4j:instance=kernel#0,name=Reports" );
					 Reports reportsBean = JMX.newMBeanProxy( _mBeanServer, name, typeof( Reports ) );
					 return reportsInvoker.Invoke( reportsBean );
				}
				catch ( MalformedObjectNameException )
				{
				}
				return "Unable to invoke ReportsBean";
			  });
		 }

		 private interface ReportsInvoker
		 {
			  string Invoke( Reports r );
		 }
	}

}