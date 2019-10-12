using System.Collections.Generic;

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
namespace Neo4Net.@internal.Collector
{

	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;

	/// <summary>
	/// Data collector section that contains meta data about the System,
	/// Neo4j deployment, graph token counts, and retrieval.
	/// </summary>
	internal sealed class MetaSection
	{
		 private MetaSection()
		 { // only static methods
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.stream.Stream<RetrieveResult> retrieve(String graphToken, org.neo4j.internal.kernel.api.Kernel kernel, long numSilentQueryDrops) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 internal static Stream<RetrieveResult> Retrieve( string graphToken, Kernel kernel, long numSilentQueryDrops )
		 {
			  IDictionary<string, object> systemData = new Dictionary<string, object>();
			  systemData["jvmMemoryFree"] = Runtime.Runtime.freeMemory();
			  systemData["jvmMemoryTotal"] = Runtime.Runtime.totalMemory();
			  systemData["jvmMemoryMax"] = Runtime.Runtime.maxMemory();
			  systemData["systemTotalPhysicalMemory"] = OsBeanUtil.TotalPhysicalMemory;
			  systemData["systemFreePhysicalMemory"] = OsBeanUtil.FreePhysicalMemory;
			  systemData["systemCommittedVirtualMemory"] = OsBeanUtil.CommittedVirtualMemory;
			  systemData["systemTotalSwapSpace"] = OsBeanUtil.TotalSwapSpace;
			  systemData["systemFreeSwapSpace"] = OsBeanUtil.FreeSwapSpace;

			  OperatingSystemMXBean os = ManagementFactory.OperatingSystemMXBean;
			  systemData["osArch"] = os.Arch;
			  systemData["osName"] = os.Name;
			  systemData["osVersion"] = os.Version;
			  systemData["availableProcessors"] = os.AvailableProcessors;
			  systemData["byteOrder"] = ByteOrder.nativeOrder().ToString();

			  RuntimeMXBean runtime = ManagementFactory.RuntimeMXBean;
			  systemData["jvmName"] = runtime.VmName;
			  systemData["jvmVendor"] = runtime.VmVendor;
			  systemData["jvmVersion"] = runtime.VmVersion;
			  CompilationMXBean compiler = ManagementFactory.CompilationMXBean;
			  systemData["jvmJITCompiler"] = compiler == null ? "unknown" : compiler.Name;

			  systemData["userLanguage"] = Locale.Default.Language;
			  systemData["userCountry"] = Locale.Default.Country;
			  systemData["userTimezone"] = TimeZone.Default.ID;
			  systemData["fileEncoding"] = System.getProperty( "file.encoding" );

			  IDictionary<string, object> internalData = new Dictionary<string, object>();
			  internalData["numSilentQueryCollectionMisses"] = numSilentQueryDrops;

			  IDictionary<string, object> metaData = new Dictionary<string, object>();
			  metaData["graphToken"] = graphToken;
			  metaData["retrieveTime"] = ZonedDateTime.now();
			  metaData["system"] = systemData;
			  metaData["internal"] = internalData;

			  TokensSection.PutTokenCounts( metaData, kernel );

			  return Stream.of( new RetrieveResult( Sections.META, metaData ) );
		 }
	}

}