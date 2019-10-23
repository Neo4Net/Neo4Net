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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;

	internal abstract class IndexCursor<T> where T : Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor
	{
		 private T _progressor;

		 internal void Initialize( T progressor )
		 {
			  if ( this._progressor != null )
			  {
					this._progressor.close();
			  }
			  this._progressor = progressor;
		 }

		 internal bool InnerNext()
		 {
			  return _progressor != null && _progressor.next();
		 }

		 internal virtual void Close()
		 {
			  if ( _progressor != null )
			  {
					_progressor.close();
			  }
			  _progressor = null;
		 }

		 internal virtual bool Closed
		 {
			 get
			 {
				  return _progressor == null;
			 }
		 }
	}

}