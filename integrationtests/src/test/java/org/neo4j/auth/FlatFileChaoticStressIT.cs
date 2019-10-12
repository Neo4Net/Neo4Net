using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.auth
{
	using Rule = org.junit.Rule;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using UTF8 = Org.Neo4j.@string.UTF8;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

	public class FlatFileChaoticStressIT : FlatFileStressBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 internal override FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return FsRule.get();
			 }
		 }

		 internal override ExecutorService SetupWorkload( int n )
		 {
			  ExecutorService service = Executors.newFixedThreadPool( n );
			  ISet<string> usernames = MakeWithPrefix( "user", n );
			  ISet<string> roleNames = MakeWithPrefix( "role", n );
			  for ( int i = 0; i < n; i++ )
			  {
					service.submit( new ChaoticAdmin( this, i, usernames, roleNames ) );
			  }
			  return service;
		 }

		 private ISet<string> MakeWithPrefix( string prefix, int n )
		 {
			  ISet<string> set = new LinkedHashSet<string>();
			  IntStream.range( 0, n ).forEach( i => set.Add( prefix + i ) );
			  return set;
		 }

		 private class ChaoticAdmin : IrrationalAdmin
		 {
			 private readonly FlatFileChaoticStressIT _outerInstance;

			  internal readonly int Number;
			  internal readonly string[] Usernames;
			  internal readonly string[] RoleNames;

			  internal ChaoticAdmin( FlatFileChaoticStressIT outerInstance, int number, ISet<string> usernames, ISet<string> roleNames ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.Number = number;
					this.Usernames = usernames.toArray( new string[usernames.Count] );
					this.RoleNames = roleNames.toArray( new string[roleNames.Count] );
					setActions( this.createUser, this.deleteUser, this.changePassword, this.suspend, this.activate, this.createRole, this.deleteRole, this.assignRole, this.unAssignRole );
			  }

			  public override string ToString()
			  {
					return "ChaoticAdmin " + Number;
			  }

			  // __________ ACTIONS ___________

			  internal virtual void CreateUser()
			  {
					string username = RandomUser();
					string password = DeviousPassword();
					try
					{
						 outerInstance.FlatFileRealm.newUser( username, UTF8.encode( password ), false );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void DeleteUser()
			  {
					string username = RandomUser();
					try
					{
						 outerInstance.FlatFileRealm.deleteUser( username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void ChangePassword()
			  {
					string username = RandomUser();
					string password = DeviousPassword();
					try
					{
						 outerInstance.FlatFileRealm.setUserPassword( username, UTF8.encode( password ), false );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void Suspend()
			  {
					string username = RandomUser();
					try
					{
						 outerInstance.FlatFileRealm.suspendUser( username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void Activate()
			  {
					string username = RandomUser();
					try
					{
						 outerInstance.FlatFileRealm.activateUser( username, false );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void CreateRole()
			  {
					string username = RandomUser();
					string roleName = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.newRole( roleName, username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void DeleteRole()
			  {
					string roleName = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.deleteRole( roleName );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void AssignRole()
			  {
					string username = RandomUser();
					string roleName = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.addRoleToUser( roleName, username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  internal virtual void UnAssignRole()
			  {
					string username = RandomUser();
					string roleName = RandomRole();
					try
					{
						 outerInstance.FlatFileRealm.removeRoleFromUser( roleName, username );
					}
					catch ( IOException e )
					{
						 outerInstance.Errors.Add( e );
					}
					catch ( InvalidArgumentsException )
					{
						 // ignore
					}
			  }

			  // ______________ HELPERS ______________

			  internal virtual string DeviousPassword()
			  {
					return Random.nextBoolean() ? "123" : "321";
			  }

			  internal virtual string RandomUser()
			  {
					return Usernames[Random.Next( Usernames.Length )];
			  }

			  internal virtual string RandomRole()
			  {
					return RoleNames[Random.Next( RoleNames.Length )];
			  }
		 }
	}

}