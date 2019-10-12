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
namespace Neo4Net.Kernel.impl.store.id.configuration
{

	/// <summary>
	/// Configuration for any specific id type </summary>
	/// <seealso cref= IdType </seealso>
	/// <seealso cref= IdTypeConfigurationProvider </seealso>
	public class IdTypeConfiguration
	{
		 internal const int DEFAULT_GRAB_SIZE = 1024;
		 internal const int AGGRESSIVE_GRAB_SIZE = 50000;

		 private readonly bool _allowAggressiveReuse;

		 public IdTypeConfiguration( bool allowAggressiveReuse )
		 {
			  this._allowAggressiveReuse = allowAggressiveReuse;
		 }

		 public virtual bool AllowAggressiveReuse()
		 {
			  return _allowAggressiveReuse;
		 }

		 public virtual int GrabSize
		 {
			 get
			 {
				  return _allowAggressiveReuse ? AGGRESSIVE_GRAB_SIZE : DEFAULT_GRAB_SIZE;
			 }
		 }
	}

}