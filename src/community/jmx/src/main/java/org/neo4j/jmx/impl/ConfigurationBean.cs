using System;
using System.Collections.Generic;

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
namespace Neo4Net.Jmx.impl
{

	using ConfigValue = Neo4Net.Configuration.ConfigValue;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;

	[Description("The configuration parameters used to configure Neo4Net"), Obsolete]
	public sealed class ConfigurationBean : Neo4NetMBean
	{
		 public const string CONFIGURATION_MBEAN_NAME = "Configuration";
		 private readonly IDictionary<string, ConfigValue> _config;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConfigurationBean(org.Neo4Net.kernel.internal.KernelData kernel, ManagementSupport support) throws javax.management.NotCompliantMBeanException
		 internal ConfigurationBean( KernelData kernel, ManagementSupport support ) : base( CONFIGURATION_MBEAN_NAME, kernel, support )
		 {
			  this._config = kernel.Config.ConfigValues;
		 }

		 private string DescribeConfigParameter( string param )
		 {
			  return _config[param].description().orElse("Configuration attribute");
		 }

		 private MBeanAttributeInfo[] Keys()
		 {
			  IList<MBeanAttributeInfo> keys = new List<MBeanAttributeInfo>();
			  foreach ( string key in _config.Keys )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					keys.Add( new MBeanAttributeInfo( key, typeof( string ).FullName, DescribeConfigParameter( key ), true, false, false ) );
			  }
			  return keys.ToArray();
		 }

		 public override object GetAttribute( string attribute )
		 {
			  return _config[attribute].valueAsString().orElse(null);
		 }

		 public override AttributeList GetAttributes( string[] attributes )
		 {
			  AttributeList result = new AttributeList( attributes.Length );
			  foreach ( string attribute in attributes )
			  {
					try
					{
						 result.add( new Attribute( attribute, GetAttribute( attribute ) ) );
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAttribute(javax.management.Attribute attribute) throws javax.management.InvalidAttributeValueException
		 public override Attribute Attribute
		 {
			 set
			 {
				  throw new InvalidAttributeValueException( "Not a writable attribute: " + value.Name );
			 }
		 }

		 public override MBeanInfo MBeanInfo
		 {
			 get
			 {
				  Description description = this.GetType().getAnnotation(typeof(Description));
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return new MBeanInfo( this.GetType().FullName, description != null ? description.value() : "Neo4Net configuration", Keys(), null, null, null );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(String s, Object[] objects, String[] strings) throws javax.management.MBeanException
		 public override object Invoke( string s, object[] objects, string[] strings )
		 {
			  try
			  {
					return this.GetType().GetMethod(s).invoke(this);
			  }
			  catch ( InvocationTargetException e )
			  {
					throw new MBeanException( ( Exception ) e.TargetException );
			  }
			  catch ( Exception e )
			  {
					throw new MBeanException( e );
			  }
		 }
	}

}