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
namespace Neo4Net.Kernel.impl.transaction.log
{
	/// <summary>
	/// Mutable marker that can create immutable <seealso cref="LogPosition"/> objects when requested to.
	/// </summary>
	public class LogPositionMarker
	{
		 private long _logVersion;
		 private long _byteOffset;
		 private bool _specified;

		 public virtual void Mark( long logVersion, long byteOffset )
		 {
			  this._logVersion = logVersion;
			  this._byteOffset = byteOffset;
			  this._specified = true;
		 }

		 public virtual void Unspecified()
		 {
			  _specified = false;
		 }

		 public virtual LogPosition NewPosition()
		 {
			  return _specified ? new LogPosition( _logVersion, _byteOffset ) : LogPosition.UNSPECIFIED;
		 }

		 public virtual long LogVersion
		 {
			 get
			 {
				  return _logVersion;
			 }
		 }

		 public virtual long ByteOffset
		 {
			 get
			 {
				  return _byteOffset;
			 }
		 }

		 public override string ToString()
		 {
			  return "Mark:" + NewPosition().ToString();
		 }
	}

}