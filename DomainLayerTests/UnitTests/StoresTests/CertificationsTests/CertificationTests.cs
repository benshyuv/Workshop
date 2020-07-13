using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayerTests.UnitTests.StoresTests.CertificationsTests
{
	class CertificationTests
	{
        Guid TEST_GRANTOR = Guid.NewGuid();
        Guid TEST_USER = Guid.NewGuid();
        Guid TEST_STORE = Guid.NewGuid();
        Certification FirstOwnerCert;
        Certification OwnerCert;
        Certification ManagerCert;
        ISet<Permission> permissions;

        [SetUp]
        public void Setup()
        {
            FirstOwnerCert = new Certification(TEST_USER, TEST_STORE, Guid.Empty, true, new HashSet<Permission>());
            OwnerCert = new Certification(TEST_USER, TEST_STORE, TEST_GRANTOR, true, new HashSet<Permission>());
            permissions = new HashSet<Permission>
            {
                Permission.HISTORY,
                Permission.REQUESTS
            };
            ManagerCert = new Certification(TEST_USER, TEST_STORE, TEST_GRANTOR, false, permissions);
        }

        [Test]
        public void IsOwner_FirstOwner_ShouldPass()
        {
            Assert.True(FirstOwnerCert.IsOwner());
        }

        [Test]
        public void IsOwner_Owner_ShouldPass()
        {
            Assert.True(OwnerCert.IsOwner());
        }

        [Test]
        public void IsOwner_Manager_ShouldFail()
        {
            Assert.False(ManagerCert.IsOwner());
        }

        [Test]
        public void IsFirstOwner_FirstOwner_ShouldPass()
        {
            Assert.True(FirstOwnerCert.IsFirstOwner());
        }

        [Test]
        public void IsFirstOwner_Owner_ShouldFail()
        {
            Assert.False(OwnerCert.IsFirstOwner());
        }

        [Test]
        public void IsFirstOwner_Manager_ShouldFail()
        {
            Assert.False(ManagerCert.IsFirstOwner());
        }

        [Test]
        public void IsGrantor_CorrectGrantor_ShouldPass()
        {
            Assert.True(OwnerCert.IsGrantor(TEST_GRANTOR));
            Assert.True(ManagerCert.IsGrantor(TEST_GRANTOR));
        }

        [Test]
        public void IsGrantor_IncorrectGrantor_ShouldFail()
        {
            Assert.False(OwnerCert.IsGrantor(Guid.NewGuid()));
        }

        [Test]
        public void AddPermission_Manager_NewPermission_ShouldPass()
        {
            Assert.DoesNotThrow(() => ManagerCert.AddPermission(Permission.INVENTORY));
        }

        [Test]
        public void AddPermission_Manager_ExistingPermission_ShouldFail()
        {
            Assert.Throws<PermissionException>(() => ManagerCert.AddPermission(Permission.REQUESTS));
        }

        [Test]
        public void AddPermission_Owner_ShouldFail()
        {
            Assert.Throws<PermissionException>(() => OwnerCert.AddPermission(Permission.INVENTORY));
        }

        [Test]
        public void RemovePermission_Manager_ExistingPermission_ShouldPass()
        {
            Assert.DoesNotThrow(() => ManagerCert.RemovePermission(Permission.REQUESTS));
        }

        [Test]
        public void RemovePermission_Manager_NewPermission_ShouldFail()
        {
            Assert.Throws<PermissionException>(() => ManagerCert.RemovePermission(Permission.INVENTORY));
        }

        [Test]
        public void RemovePermission_Owner_ShouldFail()
        {
            Assert.Throws<PermissionException>(() => OwnerCert.RemovePermission(Permission.INVENTORY));
        }

        [Test]
        public void IsPermitted_Owner_ShouldPass()
        {
            Assert.True(OwnerCert.IsPermitted(Permission.APPOINT_OWNER));
        }

        [Test]
        public void IsPermitted_Manager_HasPermission_ShouldPass()
        {
            Assert.True(ManagerCert.IsPermitted(Permission.HISTORY));
        }

        [Test]
        public void IsPermitted_Manager_NoPermission_ShouldFail()
        {
            Assert.False(ManagerCert.IsPermitted(Permission.APPOINT_OWNER));
        }

        [Test]
        public void GetPermissions_Manager_Limited()
        {
            ISet<Permission> permissions = ManagerCert.Permissions;
            Assert.AreEqual(2, permissions.Count);
            Assert.True(permissions.Contains(Permission.HISTORY) & permissions.Contains(Permission.REQUESTS));
        }

        [Test]
        public void GetPermissions_Owner_All()
        {
            ISet<Permission> permissions = OwnerCert.Permissions;
            Assert.AreEqual(Enum.GetValues(typeof(Permission)).Length, permissions.Count);
            foreach (Permission p in Enum.GetValues(typeof(Permission)))
            {
                Assert.IsTrue(permissions.Contains(p));
            }
        }
    }
}
