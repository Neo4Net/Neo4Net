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
namespace Neo4Net.Logging.@internal
{

	public class SimpleLogService : AbstractLogService
	{
		 private readonly LogProvider _userLogProvider;
		 private readonly LogProvider _internalLogProvider;

		 /// <summary>
		 /// Create log service where both: user and internal log provider use the same <seealso cref="LogProvider"/> as a provider.
		 /// Should be used when user and internal are backed by same log provider. </summary>
		 /// <param name="commonLogProvider"> log provider </param>
		 public SimpleLogService( LogProvider commonLogProvider )
		 {
			  this._userLogProvider = commonLogProvider;
			  this._internalLogProvider = commonLogProvider;
		 }

		 /// <summary>
		 /// Create log service with different user and internal log providers.
		 /// User logs will be duplicated to internal logs as well.
		 /// Should be used when user and internal are backed by different log providers. </summary>
		 /// <param name="userLogProvider"> user log provider </param>
		 /// <param name="internalLogProvider"> internal log provider </param>
		 public SimpleLogService( LogProvider userLogProvider, LogProvider internalLogProvider )
		 {
			  this._userLogProvider = new DuplicatingLogProvider( userLogProvider, internalLogProvider );
			  this._internalLogProvider = internalLogProvider;
		 }

		 public override LogProvider UserLogProvider
		 {
			 get
			 {
				  return this._userLogProvider;
			 }
		 }

		 public override LogProvider InternalLogProvider
		 {
			 get
			 {
				  return this._internalLogProvider;
			 }
		 }
	}

}