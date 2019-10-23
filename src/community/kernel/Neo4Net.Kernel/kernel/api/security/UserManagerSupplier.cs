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
namespace Neo4Net.Kernel.api.security
{
	using AuthSubject = Neo4Net.Kernel.Api.Internal.security.AuthSubject;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	public interface UserManagerSupplier : Lifecycle
	{
		 UserManager GetUserManager( AuthSubject authSubject, bool isUserManager );

		 UserManager UserManager { get; }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 UserManagerSupplier NO_AUTH = new UserManagerSupplier()
	//	 {
	//		  @@Override public void init()
	//		  {
	//		  }
	//
	//		  @@Override public void start()
	//		  {
	//		  }
	//
	//		  @@Override public void stop()
	//		  {
	//		  }
	//
	//		  @@Override public void shutdown()
	//		  {
	//		  }
	//
	//		  @@Override public UserManager getUserManager(AuthSubject authSubject, boolean isUserManager)
	//		  {
	//				return UserManager.NO_AUTH;
	//		  }
	//
	//		  @@Override public UserManager getUserManager()
	//		  {
	//				return UserManager.NO_AUTH;
	//		  }
	//	 };
	}

}