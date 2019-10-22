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
namespace Neo4Net.Kernel.Impl.Api.scan
{

	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;

	/// <summary>
	/// Stream of changes used to rebuild a <seealso cref="LabelScanStore"/> from scratch.
	/// </summary>
	public interface FullStoreChangeStream
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long applyTo(org.Neo4Net.kernel.api.labelscan.LabelScanWriter writer) throws java.io.IOException;
		 long ApplyTo( LabelScanWriter writer );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static FullStoreChangeStream asStream(final java.util.List<org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate> existingData)
	//	 {
	//		  return writer ->
	//		  {
	//				long count = 0;
	//				for (NodeLabelUpdate update : existingData)
	//				{
	//					 writer.write(update);
	//					 count++;
	//				}
	//				return count;
	//		  };
	//	 }
	}

	public static class FullStoreChangeStream_Fields
	{
		 public static readonly FullStoreChangeStream Empty = writer => 0;
	}

}