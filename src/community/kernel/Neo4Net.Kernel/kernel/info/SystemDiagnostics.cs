using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.Kernel.info
{

	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using DiagnosticsPhase = Neo4Net.Internal.Diagnostics.DiagnosticsPhase;
	using DiagnosticsProvider = Neo4Net.Internal.Diagnostics.DiagnosticsProvider;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using Logger = Neo4Net.Logging.Logger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;

	public abstract class SystemDiagnostics : DiagnosticsProvider
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SYSTEM_MEMORY("System memory information:") { void dump(org.Neo4Net.logging.Logger logger) { logBytes(logger, "Total Physical memory: ", org.Neo4Net.io.os.OsBeanUtil.getTotalPhysicalMemory()); logBytes(logger, "Free Physical memory: ", org.Neo4Net.io.os.OsBeanUtil.getFreePhysicalMemory()); logBytes(logger, "Committed virtual memory: ", org.Neo4Net.io.os.OsBeanUtil.getCommittedVirtualMemory()); logBytes(logger, "Total swap space: ", org.Neo4Net.io.os.OsBeanUtil.getTotalSwapSpace()); logBytes(logger, "Free swap space: ", org.Neo4Net.io.os.OsBeanUtil.getFreeSwapSpace()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       JAVA_MEMORY("JVM memory information:") { void dump(org.Neo4Net.logging.Logger logger) { logger.log("Free  memory: " + bytes(Runtime.getRuntime().freeMemory())); logger.log("Total memory: " + bytes(Runtime.getRuntime().totalMemory())); logger.log("Max   memory: " + bytes(Runtime.getRuntime().maxMemory())); for(GarbageCollectorMXBean gc : ManagementFactory.getGarbageCollectorMXBeans()) { logger.log("Garbage Collector: " + gc.getName() + ": " + java.util.Arrays.toString(gc.getMemoryPoolNames())); } for(MemoryPoolMXBean pool : ManagementFactory.getMemoryPoolMXBeans()) { MemoryUsage usage = pool.getUsage(); logger.log(String.format("Memory Pool: %s (%s): committed=%s, used=%s, max=%s, threshold=%s", pool.getName(), pool.getType(), usage == null ? "?" : bytes(usage.getCommitted()), usage == null ? "?" : bytes(usage.getUsed()), usage == null ? "?" : bytes(usage.getMax()), pool.isUsageThresholdSupported() ? bytes(pool.getUsageThreshold()) : "?")); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       OPERATING_SYSTEM("Operating system information:") { void dump(org.Neo4Net.logging.Logger logger) { OperatingSystemMXBean os = ManagementFactory.getOperatingSystemMXBean(); RuntimeMXBean runtime = ManagementFactory.getRuntimeMXBean(); logger.log(String.format("Operating System: %s; version: %s; arch: %s; cpus: %s", os.getName(), os.getVersion(), os.getArch(), os.getAvailableProcessors())); logLong(logger, "Max number of file descriptors: ", org.Neo4Net.io.os.OsBeanUtil.getMaxFileDescriptors()); logLong(logger, "Number of open file descriptors: ", org.Neo4Net.io.os.OsBeanUtil.getOpenFileDescriptors()); logger.log("Process id: " + runtime.getName()); logger.log("Byte order: " + java.nio.ByteOrder.nativeOrder()); logger.log("Local timezone: " + getLocalTimeZone()); } private String getLocalTimeZone() { java.util.TimeZone tz = java.util.Calendar.getInstance().getTimeZone(); return tz.getID(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       JAVA_VIRTUAL_MACHINE("JVM information:") { void dump(org.Neo4Net.logging.Logger logger) { RuntimeMXBean runtime = ManagementFactory.getRuntimeMXBean(); logger.log("VM Name: " + runtime.getVmName()); logger.log("VM Vendor: " + runtime.getVmVendor()); logger.log("VM Version: " + runtime.getVmVersion()); CompilationMXBean compiler = ManagementFactory.getCompilationMXBean(); logger.log("JIT compiler: " + ((compiler == null) ? "unknown" : compiler.getName())); logger.log("VM Arguments: " + runtime.getInputArguments()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CLASSPATH("Java classpath:") { void dump(org.Neo4Net.logging.Logger logger) { RuntimeMXBean runtime = ManagementFactory.getRuntimeMXBean(); java.util.Collection<String> classpath; if(runtime.isBootClassPathSupported()) { classpath = buildClassPath(getClass().getClassLoader(), new String[] { "bootstrap", "classpath" }, runtime.getBootClassPath(), runtime.getClassPath()); } else { classpath = buildClassPath(getClass().getClassLoader(), new String[] { "classpath" }, runtime.getClassPath()); } for(String path : classpath) { logger.log(path); } } private java.util.Collection<String> buildClassPath(ClassLoader loader, String[] pathKeys, String... classPaths) { java.util.Map<String, String> paths = new java.util.HashMap<>(); assert pathKeys.length == classPaths.length; for(int i = 0; i < classPaths.length; i++) { for(String path : classPaths[i].split(java.io.File.pathSeparator)) { paths.put(canonicalize(path), pathValue(paths, pathKeys[i], path)); } } for(int level = 0; loader != null; level++) { if(loader instanceof java.net.URLClassLoader) { java.net.URLClassLoader urls = (java.net.URLClassLoader) loader; URL[] classLoaderUrls = urls.getURLs(); if(classLoaderUrls != null) { for(java.net.URL url : classLoaderUrls) { if("file".equalsIgnoreCase(url.getProtocol())) { paths.put(url.toString(), pathValue(paths, "loader." + level, url.getPath())); } } } else { paths.put(loader.toString(), "<ClassLoader unexpectedly has null URL array>"); } } loader = loader.getParent(); } java.util.List<String> result = new java.util.ArrayList<>(paths.size()); for(java.util.Map.Entry<String, String> path : paths.entrySet()) { result.add(" [" + path.getValue() + "] " + path.getKey()); } return result; } private String pathValue(java.util.Map<String, String> paths, String key, String path) { String value; if(null != (value = paths.remove(canonicalize(path)))) { value += " + " + key; } else { value = key; } return value; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LIBRARY_PATH("Library path:") { void dump(org.Neo4Net.logging.Logger logger) { RuntimeMXBean runtime = ManagementFactory.getRuntimeMXBean(); for(String path : runtime.getLibraryPath().split(java.io.File.pathSeparator)) { logger.log(canonicalize(path)); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SYSTEM_PROPERTIES("System.properties:") { void dump(org.Neo4Net.logging.Logger logger) { for(Object property : System.getProperties().keySet()) { if(property instanceof String) { String key = (String) property; if(key.startsWith("java.") || key.startsWith("os.") || key.endsWith(".boot.class.path") || key.equals("line.separator")) { continue; } logger.log(key + " = " + System.getProperty(key)); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TIMEZONE_DATABASE("(IANA) TimeZone Database Version:") { void dump(org.Neo4Net.logging.Logger logger) { java.util.Map<String, int> versions = new java.util.HashMap<>(); for(String tz : java.time.zone.ZoneRulesProvider.getAvailableZoneIds()) { for(String version : java.time.zone.ZoneRulesProvider.getVersions(tz).keySet()) { versions.compute(version, (key, value) -> value == null ? 1 : (value + 1)); } } String[] sorted = versions.keySet().toArray(new String[0]); java.util.Arrays.sort(sorted); for(String tz : sorted) { logger.log("  TimeZone version: %s (available for %d zone identifiers)", tz, versions.get(tz)); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LINUX_SCHEDULERS("Linux scheduler information:") { private final java.io.File SYS_BLOCK = new java.io.File("/sys/block"); boolean isApplicable() { return SYS_BLOCK.isDirectory(); } void dump(org.Neo4Net.logging.Logger logger) { File[] files = SYS_BLOCK.listFiles(java.io.File::isDirectory); if(files != null) { for(java.io.File subdir : files) { java.io.File scheduler = new java.io.File(subdir, "queue/scheduler"); if(scheduler.isFile()) { try(java.util.stream.Stream<String> lines = java.nio.file.Files.lines(scheduler.toPath())) { lines.forEach(logger::log); } catch(java.io.IOException e) { } } } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NETWORK("Network information:") { void dump(org.Neo4Net.logging.Logger logger) { try { java.util.Iterator<java.net.NetworkInterface> networkInterfaces = getNetworkInterfaces(); while (networkInterfaces.hasMoreElements()) { java.net.NetworkInterface iface = networkInterfaces.nextElement(); logger.log(String.format("Interface %s:", iface.getDisplayName())); java.util.Iterator<java.net.InetAddress> addresses = iface.getInetAddresses(); while (addresses.hasMoreElements()) { java.net.InetAddress address = addresses.nextElement(); String hostAddress = address.getHostAddress(); logger.log("    address: %s", hostAddress); } } } catch(java.net.SocketException e) { logger.log("ERROR: failed to inspect network interfaces and addresses: " + e.getMessage()); } } };

		 private static readonly IList<SystemDiagnostics> valueList = new List<SystemDiagnostics>();

		 static SystemDiagnostics()
		 {
			 valueList.Add( SYSTEM_MEMORY );
			 valueList.Add( JAVA_MEMORY );
			 valueList.Add( OPERATING_SYSTEM );
			 valueList.Add( JAVA_VIRTUAL_MACHINE );
			 valueList.Add( CLASSPATH );
			 valueList.Add( LIBRARY_PATH );
			 valueList.Add( SYSTEM_PROPERTIES );
			 valueList.Add( TIMEZONE_DATABASE );
			 valueList.Add( LINUX_SCHEDULERS );
			 valueList.Add( NETWORK );
		 }

		 public enum InnerEnum
		 {
			 SYSTEM_MEMORY,
			 JAVA_MEMORY,
			 OPERATING_SYSTEM,
			 JAVA_VIRTUAL_MACHINE,
			 CLASSPATH,
			 LIBRARY_PATH,
			 SYSTEM_PROPERTIES,
			 TIMEZONE_DATABASE,
			 LINUX_SCHEDULERS,
			 NETWORK
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private SystemDiagnostics( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly string message;

		 internal SystemDiagnostics( string name, InnerEnum innerEnum, string message )
		 {
			  this._message = message;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static void RegisterWith( Neo4Net.Internal.Diagnostics.DiagnosticsManager manager )
		 {
			  foreach ( SystemDiagnostics provider in values() )
			  {
					if ( provider.Applicable )
					{
						 manager.AppendProvider( provider );
					}
			  }
		 }

		 internal bool Applicable
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public string DiagnosticsIdentifier
		 {
			 get
			 {
				  return name();
			 }
		 }

		 public void AcceptDiagnosticsVisitor( object visitor )
		 {
			  // nothing visits this
		 }

		 public void Dump( Neo4Net.Internal.Diagnostics.DiagnosticsPhase phase, Neo4Net.Logging.Logger logger )
		 {
			  if ( phase.Initialization || phase.ExplicitlyRequested )
			  {
					logger.Log( _message );
					Dump( logger );
			  }
		 }

		 internal abstract void dump( Neo4Net.Logging.Logger logger );

		 private static string Canonicalize( string path )
		 {
			  try
			  {
					return ( new File( path ) ).CanonicalFile.AbsolutePath;
			  }
			  catch ( IOException )
			  {
					return Path.GetFullPath( path );
			  }
		 }

		 private static void LogBytes( Neo4Net.Logging.Logger logger, string message, long value )
		 {
			  if ( value != OsBeanUtil.VALUE_UNAVAILABLE )
			  {
					logger.Log( message + bytes( value ) );
			  }
		 }

		 private static void LogLong( Neo4Net.Logging.Logger logger, string message, long value )
		 {
			  if ( value != OsBeanUtil.VALUE_UNAVAILABLE )
			  {
					logger.Log( message + value );
			  }
		 }

		public static IList<SystemDiagnostics> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static SystemDiagnostics ValueOf( string name )
		{
			foreach ( SystemDiagnostics enumInstance in SystemDiagnostics.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}