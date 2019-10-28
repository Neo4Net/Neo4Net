﻿/*
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
namespace Neo4Net.Kernel.Api.StorageEngine
{
	public interface IStorageEntityScanCursor : StorageEntityCursor
	{
		 /// <summary>
		 /// Initializes this ICursor so that it will scan over all existing entities. Each call to <seealso cref="next()"/> will
		 /// advance the ICursor so that the next IEntity is read.
		 /// </summary>
		 void Scan();

		 /// <summary>
		 /// Initializes this ICursor so that the next call to <seealso cref="next()"/> will place this ICursor at that IEntity. </summary>
		 /// <param name="reference"> IEntity to place this ICursor at the next call to <seealso cref="next()"/>. </param>
		 void Single( long reference );
	}

}