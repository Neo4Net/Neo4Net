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
namespace Neo4Net.Kernel.Impl.Store.Records
{
	public class MetaDataRecord : AbstractBaseRecord
	{
		 private long _value;

		 public MetaDataRecord() : base(-1)
		 {
		 }

		 public virtual MetaDataRecord Initialize( bool inUse, long value )
		 {
			  base.Initialize( inUse );
			  this._value = value;
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, -1 );
		 }

		 public virtual long Value
		 {
			 get
			 {
				  return _value;
			 }
		 }
	}

}