using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.SORT_BY_NODE_ID;

	public class LabelUpdateWork : Work<System.Func<LabelScanWriter>, LabelUpdateWork>
	{
		 private readonly IList<NodeLabelUpdate> _labelUpdates;

		 public LabelUpdateWork( IList<NodeLabelUpdate> labelUpdates )
		 {
			  this._labelUpdates = labelUpdates;
		 }

		 public override LabelUpdateWork Combine( LabelUpdateWork work )
		 {
			  ( ( IList<NodeLabelUpdate> )_labelUpdates ).AddRange( work._labelUpdates );
			  return this;
		 }

		 public override void Apply( System.Func<LabelScanWriter> labelScanStore )
		 {
			  _labelUpdates.sort( SORT_BY_NODE_ID );
			  try
			  {
					  using ( LabelScanWriter writer = labelScanStore() )
					  {
						foreach ( NodeLabelUpdate update in _labelUpdates )
						{
							 writer.Write( update );
						}
					  }
			  }
			  catch ( Exception e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }
	}

}