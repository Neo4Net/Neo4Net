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
namespace Neo4Net.Kernel.impl.transaction
{
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;

	public class SimpleLogVersionRepository : LogVersionRepository
	{
		 private volatile long _logVersion;

		 public SimpleLogVersionRepository() : this(log.LogVersionRepository_Fields.INITIAL_LOG_VERSION)
		 {
		 }
		 public SimpleLogVersionRepository( long initialLogVersion )
		 {
			  this._logVersion = initialLogVersion;
		 }

		 public override long IncrementAndGetVersion()
		 {
			  _logVersion++;
			  return _logVersion;
		 }

		 public virtual long CurrentLogVersion
		 {
			 get
			 {
				  return _logVersion;
			 }
			 set
			 {
				  this._logVersion = value;
			 }
		 }

	}

}