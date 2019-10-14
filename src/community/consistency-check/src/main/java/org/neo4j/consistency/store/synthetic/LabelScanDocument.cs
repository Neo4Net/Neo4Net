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
namespace Neo4Net.Consistency.store.synthetic
{
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

	/// <summary>
	/// Synthetic record type that stands in for a real record to fit in conveniently
	/// with consistency checking
	/// </summary>
	public class LabelScanDocument : AbstractBaseRecord
	{
		 private NodeLabelRange _nodeLabelRange;

		 public LabelScanDocument( NodeLabelRange nodeLabelRange ) : base( nodeLabelRange.Id() )
		 {
			  this._nodeLabelRange = nodeLabelRange;
			  InUse = true;
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  this._nodeLabelRange = null;
		 }

		 public virtual NodeLabelRange NodeLabelRange
		 {
			 get
			 {
				  return _nodeLabelRange;
			 }
		 }

		 public override string ToString()
		 {
			  return _nodeLabelRange.ToString();
		 }
	}

}