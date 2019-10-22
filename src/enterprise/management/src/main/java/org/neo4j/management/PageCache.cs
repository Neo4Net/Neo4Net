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
namespace Neo4Net.management
{
	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	[ManagementInterface(name : PageCache_Fields.NAME), Description("Information about the Neo4Net page cache. " + "All numbers are counts and sums since the Neo4Net instance was started")]
	public interface PageCache
	{

		 [Description("Number of page faults. How often requested data was not found in memory and had to be loaded.")]
		 long Faults { get; }

		 [Description("Number of page evictions. How many pages have been removed from memory to make room for other pages.")]
		 long Evictions { get; }

		 [Description("Number of page pins. How many pages have been accessed (monitoring must be enabled separately).")]
		 long Pins { get; }

		 [Description("Number of page flushes. How many dirty pages have been written to durable storage.")]
		 long Flushes { get; }

		 [Description("Number of bytes read from durable storage.")]
		 long BytesRead { get; }

		 [Description("Number of bytes written to durable storage.")]
		 long BytesWritten { get; }

		 [Description("Number of files that have been mapped into the page cache.")]
		 long FileMappings { get; }

		 [Description("Number of files that have been unmapped from the page cache.")]
		 long FileUnmappings { get; }

		 [Description("Number of exceptions caught during page eviction. " + "This number should be zero, or at least not growing, in a healthy database. " + "Otherwise it could indicate drive failure, storage space, or permission problems.")]
		 long EvictionExceptions { get; }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
[Description("The percentage of used pages. Will return NaN if it cannot be determined.")]
//		 default double getUsageRatio()
	//	 {
	//		  return Double.NaN;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
[Description("Ratio of hits to the total number of lookups in the page cache")]
//		 default double getHitRatio()
	//	 {
	//		 return Double.NaN;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
[Description("Number of page unpins. How many pages have been accessed and are not accessed anymore (monitoring must be enabled separately).")]
//		 default long getUnpins()
	//	 {
	//		  return 0;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
[Description("Number of page hits. How often requested data was found in memory.")]
//		 default long getHits()
	//	 {
	//		  return 0;
	//	 }
	}

	public static class PageCache_Fields
	{
		 public const string NAME = "Page cache";
	}

}